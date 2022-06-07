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

namespace Origins.NPCs {
	public partial class OriginGlobalNPC : GlobalNPC {
		public override void SetDefaults(NPC npc) {
			if (Rasterized_Debuff.ID != -1) npc.buffImmune[Rasterized_Debuff.ID] = npc.buffImmune[BuffID.Confused];
		}
		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			if (type == NPCID.Demolitionist && ModContent.GetInstance<OriginSystem>().peatSold >= 20) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
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
				npc.velocity = Vector2.Lerp(npc.velocity, npc.oldVelocity, rasterizedTime * 0.0625f);
				npc.position = Vector2.Lerp(npc.position, npc.oldPosition, rasterizedTime * 0.0625f);
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
			return true;
		}
		/*public override void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) {
			knockback*=MeleeCollisionNPCData.knockbackMult;
			MeleeCollisionNPCData.knockbackMult = 1f;
		}*/
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
			Player player = spawnInfo.Player;
			if (player.GetModPlayer<OriginPlayer>().ZoneDefiled) {
				pool[0] = 0;

				pool.Add(ModContent.NPCType<Defiled_Cyclops>(), DefiledWastelands.SpawnRates.Cyclops);

				if (spawnInfo.PlayerFloorY <= Main.worldSurface + 50 && spawnInfo.SpawnTileY < Main.worldSurface - 50) pool.Add(ModContent.NPCType<Defiled_Flyer>(), DefiledWastelands.SpawnRates.Flyer * (player.ZoneSkyHeight ? 2 : 1));
				if (Main.hardMode) {
					pool.Add(ModContent.NPCType<Defiled_Hunter_Head>(), DefiledWastelands.SpawnRates.Hunter);
					if (TileID.Sets.Conversion.Sand[Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY].TileType]) {
						pool.Add(ModContent.NPCType<Defiled_Cyclops>(), DefiledWastelands.SpawnRates.Cyclops);
					}
					if (!spawnInfo.PlayerSafe) {
						pool.Add(ModContent.NPCType<Defiled_Tripod>(), DefiledWastelands.SpawnRates.Tripod);
					}
				}

				if (spawnInfo.SpawnTileY > Main.worldSurface) {
					if (!spawnInfo.PlayerSafe) {
						pool.Add(ModContent.NPCType<Defiled_Digger_Head>(), DefiledWastelands.SpawnRates.Worm);
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
					pool.Add(ModContent.NPCType<Defiled_Mite>(), DefiledWastelands.SpawnRates.Mite);
					SkipMiteSpawn:;
					if (Main.hardMode && !spawnInfo.PlayerSafe) {
						pool.Add(ModContent.NPCType<Defiled_Mimic>(), DefiledWastelands.SpawnRates.Mimic);
						if (spawnInfo.SpawnTileY > Main.rockLayer) {
							pool.Add(ModContent.NPCType<Enchanted_Trident>(), DefiledWastelands.SpawnRates.Bident);
						}
					}
				} else {
					pool.Add(ModContent.NPCType<Defiled_Brute>(), DefiledWastelands.SpawnRates.Brute);
				}
			} else if (player.GetModPlayer<OriginPlayer>().ZoneRiven) {
				pool[0] = 0;

				pool.Add(ModContent.NPCType<Riven.Riven_Fighter>(), RivenHive.SpawnRates.Fighter);

				pool.Add(ModContent.NPCType<Riven.Riven_Tank>(), RivenHive.SpawnRates.Tank);

				if (spawnInfo.Water) pool.Add(ModContent.NPCType<Riven.Pustule_Jelly>(), RivenHive.SpawnRates.Jelly);

				//if (spawnInfo.playerFloorY <= Main.worldSurface + 50 && spawnInfo.spawnTileY < Main.worldSurface - 50) pool.Add(ModContent.NPCType<Defiled_Flyer>(), DefiledWastelands.SpawnRates.Flyer * (player.ZoneSkyHeight ? 2 : 1));
				if (Main.hardMode) {
					pool.Add(ModContent.NPCType<Riven.Rivenator_Head>(), RivenHive.SpawnRates.Worm);
					if (player.ZoneDesert) {
						pool.Add(ModContent.NPCType<Riven.Riven_Mummy>(), RivenHive.SpawnRates.Mummy);
					}
					if (Terraria.GameContent.Events.Sandstorm.Happening && player.ZoneSandstorm) {
						pool.Add(ModContent.NPCType<Riven.Splitooth>(), RivenHive.SpawnRates.Shark1);
					}
				}

				/*if (spawnInfo.spawnTileY > Main.worldSurface) {
					pool.Add(ModContent.NPCType<Defiled_Digger_Head>(), DefiledWastelands.SpawnRates.Worm);
					int yPos = spawnInfo.spawnTileY;
					Tile tile;
					for (int i = 0; i < Defiled_Mite.spawnCheckDistance; i++) {
						tile = Main.tile[spawnInfo.spawnTileX, ++yPos];
						if (tile.active()) {
							yPos--;
							break;
						}
					}
					bool? halfSlab = null;
					for (int i = spawnInfo.spawnTileX - 1; i < spawnInfo.spawnTileX + 2; i++) {
						tile = Main.tile[i, yPos + 1];
						if (!tile.active() || !Main.tileSolid[tile.type] || tile.slope() != SlopeID.None || (halfSlab.HasValue && halfSlab.Value != tile.halfBrick())) {
							tile = null;
							goto SkipMiteSpawn;
						}
						halfSlab = tile.halfBrick();
					}
					tile = null;
					pool.Add(ModContent.NPCType<Defiled_Mite>(), DefiledWastelands.SpawnRates.Mite);
				SkipMiteSpawn:;
				}*/
			}
		}
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			if (player.GetModPlayer<OriginPlayer>().rapidSpawnFrames > 0) {
				spawnRate = 1;
			}
		}
	}
}
