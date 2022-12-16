using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.NPCs.Defiled;
using Origins.Tiles.Defiled;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.World.BiomeData;
using Origins.Buffs;
using Terraria.GameContent.ItemDropRules;
using Origins.Tiles.Riven;
using Origins.Items.Accessories;
using Terraria.DataStructures;
using Terraria.ModLoader.Utilities;
using Origins.NPCs.Riven;
using Origins.Tiles;

namespace Origins.NPCs {
	public partial class OriginGlobalNPC : GlobalNPC {
		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			//Demo-man
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 0) {
				shop.item[nextSlot++].SetDefaults(ItemID.ExplosivePowder);
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 5) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Peatball>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 10) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 20) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 35) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
			}
			//if statment for Hardmode here
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 50) {
				//shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Mortar_Shell>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 75) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Acid_Grenade>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 100) {
				//shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Acid_Bomb>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 120) {
				//shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Nade_O_Plenty>());
			}
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 999) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Caustica>());
			}
			//Cyborg
			if (type == NPCID.Cyborg) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Advanced_Imaging>());
			}
		}
		public override bool PreAI(NPC npc) {
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
			if (rasterizedTime > 0) {
				npc.velocity = Vector2.Lerp(npc.velocity, npc.oldVelocity, rasterizedTime * 0.0625f * 0.5f);
				npc.position = Vector2.Lerp(npc.position, npc.oldPosition, rasterizedTime * 0.0625f * 0.5f);
			}
			if (slowDebuff) {
				npc.position = Vector2.Lerp(npc.oldPosition, npc.position, 0.7f);
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				if (toxicShockTime < Toxic_Shock_Debuff.stun_duration) {
					toxicShockTime++;
					npc.position -= npc.velocity;
					return false;
				} else if (toxicShockTime <= Toxic_Shock_Debuff.stun_duration * 3) {
					toxicShockTime++;
				}
			} else if (toxicShockTime > 0) {
				toxicShockTime = 0;
			}
			if (npc.HasBuff(BuffID.OgreSpit) && (npc.collideX || npc.collideY)) {
				npc.velocity *= 0.95f;
			}
			if (infusionSpikes is object) infusionSpikes.Clear();
			return true;
		}
		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			if (npc.HasBuff(Impaled_Debuff.ID) || npc.HasBuff(Stunned_Debuff.ID)) return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}
		public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			if (npc.HasBuff(Solvent_Debuff.ID) && crit) {
				damage *= 1.3;
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				damage += defense * 0.1f;
			}
			if (tornTime > 0) {
				damage /= 1 - ((1 - tornTarget) * (tornTime / (float)tornTargetTime));
			}
			return true;
		}
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]) {
				float damageBoost = 0;
				if (npc.HasBuff(Flagellash_Buff_0.ID)) {
					damageBoost += 2.7f;
				}
				if (npc.HasBuff(Flagellash_Buff_1.ID)) {
					damageBoost += 2.65f;
				}
				if (npc.HasBuff(Flagellash_Buff_2.ID)) {
					damageBoost += 2.65f;
				}
				if (amebolizeDebuff) {
					damageBoost += 5f;
				}
				damage += Main.rand.RandomRound(damageBoost);
			}
			if (Main.expertMode) {
				if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail) {
					switch (projectile.type) {
						case ProjectileID.Bomb:
						case ProjectileID.Dynamite:
						case ProjectileID.Grenade:
						case ProjectileID.StickyBomb:
						case ProjectileID.Explosives:
						case ProjectileID.Bee://bee?? like, all of them?
						case ProjectileID.Beenade:
						case ProjectileID.ExplosiveBunny:
						case ProjectileID.StickyGrenade:
						case ProjectileID.StickyDynamite:
						case ProjectileID.BouncyBomb:
						case ProjectileID.BouncyGrenade:
						case ProjectileID.BombFish:
						case ProjectileID.GiantBee:// these too?
						case ProjectileID.PartyGirlGrenade:
						case ProjectileID.BouncyDynamite:
						case ProjectileID.ScarabBomb:
						if(!Main.masterMode) damage *= new Fraction(5, 2);
						break;
						default:
						if(projectile.CountsAsClass(DamageClasses.Explosive)) damage /= Main.masterMode ? 5 : 2;
						break;
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
			if (spawnInfo.SpawnTileType == ModContent.TileType<Tiles.Other.Fiberglass_Tile>()) {
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
			if (TileLoader.GetTile(spawnInfo.SpawnTileType) is DefiledTile) {
				if (Main.invasionType <= 0) pool[0] = 0;

				pool.Add(ModContent.NPCType<Defiled_Cyclops>(), Defiled_Wastelands.SpawnRates.Cyclops);

				if (spawnInfo.PlayerFloorY <= Main.worldSurface + 50 && spawnInfo.SpawnTileY < Main.worldSurface - 50) pool.Add(ModContent.NPCType<Defiled_Flyer>(), Defiled_Wastelands.SpawnRates.Flyer * (player.ZoneSkyHeight ? 2 : 1));
				if (Main.hardMode) {
					pool.Add(ModContent.NPCType<Defiled_Hunter_Head>(), Defiled_Wastelands.SpawnRates.Hunter);
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
				if (Defiled_Amalgamation.spawnDA) {
					foreach (var entry in pool) {
						pool[entry.Key] = 0;
					}
					if (spawnInfo.PlayerFloorY < Main.worldSurface && Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != ModContent.WallType<Walls.Defiled_Stone_Wall>()) {
						pool.Add(ModContent.NPCType<Defiled_Amalgamation>(), 999);
					}
				}
			}
			if (TileLoader.GetTile(spawnInfo.SpawnTileType) is RivenTile) {
				if (Main.invasionType <= 0) pool[0] = 0;

				pool.Add(ModContent.NPCType<Riven_Fighter>(), Riven_Hive.SpawnRates.Fighter);

				pool.Add(ModContent.NPCType<Barnacle_Mound>(), Riven_Hive.SpawnRates.Barnacle);

				if (spawnInfo.Water) pool.Add(ModContent.NPCType<Measly_Moeba>(), Riven_Hive.SpawnRates.Moeba);

				//if (spawnInfo.playerFloorY <= Main.worldSurface + 50 && spawnInfo.spawnTileY < Main.worldSurface - 50) pool.Add(ModContent.NPCType<Defiled_Flyer>(), DefiledWastelands.SpawnRates.Flyer * (player.ZoneSkyHeight ? 2 : 1));
				if (Main.hardMode) {
					pool.Add(ModContent.NPCType<Rivenator_Head>(), Riven_Hive.SpawnRates.Worm);
					if (player.ZoneDesert) {
						pool.Add(ModContent.NPCType<Riven_Mummy>(), Riven_Hive.SpawnRates.Mummy);
					}
					if (Terraria.GameContent.Events.Sandstorm.Happening && player.ZoneSandstorm) {
						//sandshark here
					}
				}
			}
			if (Main.hardMode && !spawnInfo.PlayerSafe) {
				if (spawnInfo.SpawnTileY > Main.rockLayer) {
					if (originPlayer.ZoneDefiled) {
						pool.Add(ModContent.NPCType<Defiled_Mimic>(), Defiled_Wastelands.SpawnRates.Mimic);
						pool.Add(ModContent.NPCType<Enchanted_Trident>(), Defiled_Wastelands.SpawnRates.Bident);
					}
					if (originPlayer.ZoneRiven) {
						pool.Add(ModContent.NPCType<Riven_Mimic>(), Defiled_Wastelands.SpawnRates.Mimic);
						//pool.Add(ModContent.NPCType<Enchanted_Trident>(), Defiled_Wastelands.SpawnRates.Bident);
					}
				}
			}
		}
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			if (player.GetModPlayer<OriginPlayer>().rapidSpawnFrames > 0) {
				spawnRate = 1;
			}
		}
	}
}
