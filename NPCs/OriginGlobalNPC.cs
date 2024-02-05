using AltLibrary.Common.Conditions;
using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs.Defiled;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Riven;
using Origins.Projectiles.Misc;
using Origins.Questing;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Utilities;
using static Origins.ConditionExtensions;

namespace Origins.NPCs {
	public partial class OriginGlobalNPC : GlobalNPC {
		public static ShoppingSettings ShopHelper_GetShoppingSettings(Terraria.GameContent.On_ShopHelper.orig_GetShoppingSettings orig, ShopHelper self, Player player, NPC npc) {
			ShoppingSettings settings = orig(self, player, npc);
			float discount = 0;
			switch (npc.type) {
				case NPCID.BestiaryGirl:
				if (ModContent.GetInstance<Discount_1_Quest>().Completed) {
					discount += 0.03f;
				}
				if (ModContent.GetInstance<Discount_2_Quest>().Completed) {
					discount += 0.03f;
				}
				break;
			}
			settings.PriceAdjustment *= 1 - discount;
			return settings;
		}
		public override void ModifyShop(NPCShop shop) {
			switch (shop.NpcType) {
				case NPCID.Clothier: {
					shop.Add<Pincushion>();
				}
				break;
				case NPCID.GoblinTinkerer: {
					shop.Add<Turbo_Reel>(Quest.QuestCondition<Turbo_Reel_Quest>());
					shop.Add<Gun_Glove>(Quest.QuestCondition<Gun_Glove_Quest>());
					break;
				}
				case NPCID.Merchant: {
					shop.Add<Blue_Bovine>(Quest.QuestCondition<Blue_Bovine_Quest>());
					shop.Add<Lottery_Ticket>(Quest.QuestCondition<Lottery_Ticket_Quest>());
					break;
				}
				case NPCID.Demolitionist: {
					shop.Add(ItemID.ExplosivePowder, Condition.PreHardmode);
					shop.Add<Peatball>(PeatSoldCondition(5));
					shop.Add<Flashbang>(PeatSoldCondition(10));
					shop.Add<Impact_Grenade>(PeatSoldCondition(20));
					shop.Add<Impact_Bomb>(PeatSoldCondition(35));
					shop.Add<Impact_Dynamite>(PeatSoldCondition(50), Condition.Hardmode);
					shop.Add<Alkaline_Grenade>(PeatSoldCondition(75), Condition.Hardmode);
					shop.Add<Alkaline_Bomb>(PeatSoldCondition(120), Condition.Hardmode);
					shop.Add<Caustica>(PeatSoldCondition(999), Condition.Hardmode);
					break;
				}
				case NPCID.Steampunker: {
					shop.InsertAfter<Gray_Solution>(ItemID.PurpleSolution, Condition.EclipseOrBloodMoon.And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>()));
					shop.InsertAfter<Teal_Solution>(ItemID.RedSolution, Condition.EclipseOrBloodMoon.And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>()));
					break;
				}
				case NPCID.Dryad: {
					shop.Add<Mojo_Injection>();
					shop.Add<Cleansing_Station_Item>(Quest.QuestCondition<Cleansing_Station_Quest>());
					shop.Add<Mojo_Flask>(Quest.QuestCondition<Cleansing_Station_Quest>());

					shop.InsertAfter<Dreadful_Powder>(ItemID.CorruptGrassEcho, Condition.NotRemixWorld.CommaAnd(Condition.BloodMoon).And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>()));
					shop.InsertAfter<Sentient_Powder>(ItemID.CrimsonGrassEcho, Condition.NotRemixWorld.CommaAnd(Condition.BloodMoon).And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>()));

					shop.InsertAfter<Dreadful_Powder, Defiled_Grass_Seeds>(Condition.BloodMoon.And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>()));
					shop.InsertAfter<Sentient_Powder, Riven_Grass_Seeds>(Condition.BloodMoon.And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>()));

					shop.InsertAfter<Defiled_Grass_Seeds>(ItemID.CorruptSeeds, Condition.InGraveyard.CommaAnd(Condition.Hardmode).And(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>().Not()));
					shop.InsertAfter<Riven_Grass_Seeds>(ItemID.CrimsonSeeds, Condition.InGraveyard.CommaAnd(Condition.Hardmode).And(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>().Not()));
					break;
				}
				case NPCID.Cyborg: {
					shop.Add<Advanced_Imaging>();
					break;
				}
				case NPCID.SkeletonMerchant: {
					//shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Trash_Lid>());
					break;
				}
				case NPCID.Golfer: {
					shop.Add<Baseball_Bat>(OriginsModIntegrations.AprilFools);
					break;
				}
				case NPCID.ArmsDealer: {
					shop.Add<Shardcannon>(Quest.QuestCondition<Shardcannon_Quest>());
					break;
				}
			}
		}
		public override bool PreAI(NPC npc) {
			if (npc.oldPosition == default && npc.oldVelocity == default && npc.position.LengthSquared() > 16) {
				npc.oldPosition = npc.position;
			}
			preAIVelocity = npc.velocity;
			if (shockTime > 0) {
				npc.noGravity = true;
				npc.velocity = Vector2.Zero;
				npc.position = npc.oldPosition;
				if (--shockTime == 0) {
					npc.life = 0;
					npc.checkDead();
				}
				return false;
			}
			if (npc.HasBuff(Impaled_Debuff.ID)) {
				//npc.position = npc.oldPosition;//-=npc.velocity;
				npc.velocity = Vector2.Zero;
				return false;
			}
			if (rasterizedTime > 0 && npc.oldPosition != default) {
				if (npc.velocity.Y == 0) {
					switch (npc.aiStyle) {
						case NPCAIStyleID.Slime:
						case NPCAIStyleID.King_Slime:
						case NPCAIStyleID.Queen_Slime:
						npc.oldVelocity.Y = 0;
						break;
					}
				}
				npc.velocity = Vector2.Lerp(npc.velocity, npc.oldVelocity, rasterizedTime * 0.0625f * 0.5f);
				npc.position = Vector2.Lerp(npc.position, npc.oldPosition, rasterizedTime * 0.0625f * 0.5f);
			}
			if (slowDebuff) {
				npc.position = Vector2.Lerp(npc.oldPosition, npc.position, 0.7f);
			}
			if (barnacleBuff) {
				npc.position = Vector2.Lerp(npc.oldPosition, npc.position, 1.01f);
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				if (toxicShockStunTime > 0) {
					toxicShockStunTime--;
					npc.position -= npc.velocity;
					return false;
				}
			} else if (toxicShockStunTime > 0) {
				toxicShockStunTime = 0;
			}
			if (npc.HasBuff(BuffID.OgreSpit) && (npc.collideX || npc.collideY)) {
				npc.velocity *= 0.95f;
			}
			if (infusionSpikes is object) infusionSpikes.Clear();
			return true;
		}
		public override void PostAI(NPC npc) {
			if (barnacleBuff) {
				if (Math.Abs(preAIVelocity.Y) <= 0.075f && npc.velocity.Y < preAIVelocity.Y) {
					npc.velocity.Y *= 1.2f;//
				}
			}
		}
		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			if (npc.HasBuff(Impaled_Debuff.ID) || npc.HasBuff(Stunned_Debuff.ID)) return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				modifiers.CritDamage *= 1.3f;
				modifiers.Defense -= 0.2f;
				if (npc.HasBuff(Toxic_Shock_Strengthen_Debuff.ID)) {// roughly 50% boost
					modifiers.CritDamage *= 1.1f;
					modifiers.Defense -= 0.1f;
				}
			}
			if (tornCurrentSeverity > 0) {
				modifiers.FinalDamage /= 1 - tornCurrentSeverity;
			}
			if (npc.GetGlobalNPC<OriginGlobalNPC>().barnacleBuff) {
				modifiers.Defense += 0.25f;
			}
		}
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]) {
				float damageBoost = 0;
				for (int i = 0; i < npc.buffType.Length; i++) {
					if (npc.buffTime[i] <= 0) continue;
					int buffType = npc.buffType[i];
					if (buffType == Flagellash_Buff_0.ID) {
						damageBoost += 2.7f;
					} else if (buffType == Flagellash_Buff_1.ID) {
						damageBoost += 2.65f;
					} else if (buffType == Flagellash_Buff_2.ID) {
						damageBoost += 2.65f;
					} else if (buffType == Maelstrom_Buff_Damage.ID) {
						damageBoost += npc.HasBuff(Maelstrom_Buff_Zap.ID) ? 7f : 5f;
					}
				}
				if (amebolizeDebuff) {
					damageBoost += 5f;
				}
				modifiers.FlatBonusDamage += Main.rand.RandomRound(damageBoost);
			}
			int forceCritBuff = npc.FindBuffIndex(Headphones_Buff.ID);
			if (forceCritBuff >= 0) {
				if (Main.rand.NextBool(4)) modifiers.SetCrit();
				npc.DelBuff(forceCritBuff);
			}
			if (Main.expertMode) {
				if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail) {
					switch (projectile.type) {
						case ProjectileID.Bomb:
						case ProjectileID.Dynamite:
						case ProjectileID.Grenade:
						case ProjectileID.StickyBomb:
						case ProjectileID.Explosives:
						case ProjectileID.Bee://well, is there a way to track if they came out of an explosive?
						case ProjectileID.Beenade:
						case ProjectileID.ExplosiveBunny:
						case ProjectileID.StickyGrenade:
						case ProjectileID.StickyDynamite:
						case ProjectileID.BouncyBomb:
						case ProjectileID.BouncyGrenade:
						case ProjectileID.BombFish:
						case ProjectileID.GiantBee:
						case ProjectileID.PartyGirlGrenade:
						case ProjectileID.BouncyDynamite:
						case ProjectileID.ScarabBomb:
						if (!Main.masterMode) modifiers.SourceDamage *= (float)new Fraction(5, 2);
						break;
						default:
						if (projectile.CountsAsClass(DamageClasses.Explosive)) modifiers.SourceDamage /= Main.masterMode ? 5 : 2;
						break;
					}
				}
			}
		}
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]) {
				if (npc.HasBuff(Maelstrom_Buff_Zap.ID) && projectile.owner == Main.myPlayer) {
					const float maxDist = 136 * 136;
					bool first = true;
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC currentTarget = Main.npc[i];
						if (i == npc.whoAmI || !currentTarget.CanBeChasedBy()) continue;

						Vector2 currentStart = currentTarget.Center.Clamp(npc.Hitbox);
						Vector2 currentEnd = npc.Center.Clamp(currentTarget.Hitbox);
						float currentDist = (currentEnd - currentStart).LengthSquared();

						if (currentDist < maxDist && (first || Main.rand.NextBool(3))) {
							first = false;
							Projectile.NewProjectileDirect(
								projectile.GetSource_OnHit(npc),
								currentEnd,
								default,
								Felnum_Shock_Leader.ID,
								5,
								hit.Knockback,
								projectile.owner,
								currentStart.X,
								currentStart.Y
							).ArmorPenetration = 20;
						}
					}
				}
			}
		}
		/*public override void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) {
			knockback*=MeleeCollisionNPCData.knockbackMult;
			MeleeCollisionNPCData.knockbackMult = 1f;
		}*/
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
			Player player = spawnInfo.Player;
			if (player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex) {
				return;
			}
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (spawnInfo.SpawnTileType == ModContent.TileType<Fiberglass_Tile>()) {
				if (originPlayer.ZoneFiberglass) {
					pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Sword>(), 1);
					pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Bow>(), 1);
					pool.Add(ModContent.NPCType<Fiberglass.Enchanted_Fiberglass_Pistol>(), 1);
					pool.Add(ModContent.NPCType<Fiberglass.Fiberglass_Weaver>(), 0.025f);
				}
				pool[0] = 0;
				return;
			}
			if (originPlayer.ZoneFiberglass) {
				pool[0] = 0;
				return;
			}
			if (player.InModBiome<Defiled_Wastelands>()) {
				if (spawnInfo.PlayerFloorY <= Main.worldSurface + 50 && spawnInfo.SpawnTileY < Main.worldSurface - 50) pool.Add(ModContent.NPCType<Defiled_Flyer>(), Defiled_Wastelands.SpawnRates.Flyer * (player.ZoneSkyHeight ? 2 : 1));

				if (Defiled_Amalgamation.spawnDA) {
					foreach (var entry in pool) {
						pool[entry.Key] = 0;
					}
					if (spawnInfo.PlayerFloorY < Main.worldSurface && Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != ModContent.WallType<Walls.Defiled_Stone_Wall>()) {
						pool.Add(ModContent.NPCType<Defiled_Amalgamation>(), 9999);
					}
					return;
				}
			}
			if (TileLoader.GetTile(spawnInfo.SpawnTileType) is DefiledTile) {
				if (Main.invasionType <= 0) pool[0] = 0;

				pool.Add(ModContent.NPCType<Defiled_Cyclops>(), Defiled_Wastelands.SpawnRates.Cyclops);
				pool.Add(ModContent.NPCType<Chunky_Slime>(), Defiled_Wastelands.SpawnRates.ChunkSlime);

				if (NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee || NPC.downedDeerclops || Main.hardMode) pool.Add(ModContent.NPCType<Ancient_Defiled_Cyclops>(), Defiled_Wastelands.SpawnRates.AncientCyclops);

				if (Main.hardMode) {
					if (TileID.Sets.Conversion.Sand[Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY].TileType]) {
						pool.Add(ModContent.NPCType<Shattered_Mummy>(), Defiled_Wastelands.SpawnRates.Cyclops);
					}
					if (!spawnInfo.PlayerSafe) {
						pool.Add(ModContent.NPCType<Defiled_Tripod>(), Defiled_Wastelands.SpawnRates.Tripod);
					}
				}

				if (spawnInfo.SpawnTileY > Main.worldSurface) {
					if (!spawnInfo.PlayerSafe) {
						pool.Add(ModContent.NPCType<Defiled_Digger_Head>(), Defiled_Wastelands.SpawnRates.Worm);
					}
					int yPos = spawnInfo.SpawnTileY;
					Tile tile;
					for (int i = 0; i < Defiled_Mite.spawnCheckDistance; i++) {
						tile = Main.tile[spawnInfo.SpawnTileX, ++yPos];
						if (tile.HasTile) {
							yPos--;
							break;
						}
					}
					bool? halfSlab = null;
					for (int i = spawnInfo.SpawnTileX - 1; i < spawnInfo.SpawnTileX + 2; i++) {
						tile = Main.tile[i, yPos + 1];
						if (!tile.HasTile || !Main.tileSolid[tile.TileType] || tile.Slope != SlopeID.None || (halfSlab.HasValue && halfSlab.Value != tile.IsHalfBlock)) {
							goto SkipMiteSpawn;
						}
						halfSlab = tile.IsHalfBlock;
					}
					pool.Add(ModContent.NPCType<Defiled_Mite>(), Defiled_Wastelands.SpawnRates.Mite);
					SkipMiteSpawn:;
				} else {
					pool.Add(ModContent.NPCType<Defiled_Brute>(), Defiled_Wastelands.SpawnRates.Brute);
				}
			}
			if (TileLoader.GetTile(spawnInfo.SpawnTileType) is RivenTile || player.InModBiome<Riven_Hive>()) {
				if (Main.invasionType <= 0) pool[0] = 0;
			}
			if (Main.hardMode && !spawnInfo.PlayerSafe) {
				if (spawnInfo.SpawnTileY > Main.rockLayer) {
					if (player.InModBiome<Defiled_Wastelands>()) {
						pool.Add(ModContent.NPCType<Defiled_Mimic>(), Defiled_Wastelands.SpawnRates.Mimic);
						pool.Add(ModContent.NPCType<Enchanted_Trident>(), Defiled_Wastelands.SpawnRates.Bident);
					}
					if (player.InModBiome<Riven_Hive>()) {
						//pool.Add(ModContent.NPCType<Riven_Mimic>(), Defiled_Wastelands.SpawnRates.Mimic);
						//pool.Add(ModContent.NPCType<Enchanted_Trident>(), Defiled_Wastelands.SpawnRates.Bident);
					}
				}
			}
		}
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.necroSet) {
				spawnRate = (int)(spawnRate * 0.5);
				maxSpawns = (int)(maxSpawns * 2f);
			}
			if (originPlayer.rapidSpawnFrames > 0) {
				spawnRate = 1;
			}
			if (player.InModBiome<Defiled_Wastelands>() || player.InModBiome<Riven_Hive>()) {
				spawnRate = (int)(spawnRate * 0.65);
				maxSpawns = (int)(maxSpawns * 1.3f);
			}
		}
		public static Condition PeatSoldCondition(int amount) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.PeatSoldCondition").WithFormatArgs(amount),
				() => ModContent.GetInstance<OriginSystem>().peatSold >= amount
			);
		}
	}
}
