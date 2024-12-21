using Microsoft.Xna.Framework;
using Origins.Questing;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.NPCs.BossThePrimordials;

namespace Origins.NPCs.MiscE.Quests {
	public class Jacob_Marley : ModNPC {
		public override string Texture => "Terraria/Images/NPC_" + NPCID.Ghost;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Ghost];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.SpawnsWithCustomName[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Ghost);
			NPC.rarity += 2;
			NPC.lifeMax = 200;
			NPC.defense = 28;
			NPC.damage = 30;
			NPC.chaseable = false;
			AIType = NPCID.Ghost;
			AnimationType = NPCID.Ghost;
			Banner = Item.NPCtoBanner(NPCID.Ghost);
		}
		public override List<string> SetNPCNameList() {
			List<string> names = LanguageManager.Instance.GetLocalizedEntriesInCategory("MerchantNames");
			names.Add("Jacob Marley");
			return names;
		}
		public override bool PreAI() {
			if (NPC.aiAction == 1) {
				Tax_Collector_Ghosts_Quest.GetTime(out int hour, out _);
				if (hour != 0 && ++NPC.alpha > 255) NPC.active = false;
			}
			NPC.aiAction = 1;
			if (NPC.life == NPC.lifeMax) {
				if (NPC.ai[3] == -1000) {
					NPC.ai[3] = 0;
					NPC.aiAction = 1;
				}
				float oldValue = MathF.Sin(NPC.ai[3]);
				NPC.ai[3] += 0.02f;
				NPC.velocity.Y = (MathF.Sin(NPC.ai[3]) - oldValue) * 8;
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.type == NPCID.TaxCollector && other.Center.IsWithin(NPC.Center, 16 * 20)) {
						NPC.spriteDirection = NPC.direction = Math.Sign(other.Center.X - NPC.Center.X);
						break;
					}
				}
				return false;
			}
			NPC.chaseable = true;
			return true;
		}
		public override void AI() {
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Player.ZoneDungeon && !NPC.AnyNPCs(Type) && ModContent.GetInstance<Tax_Collector_Ghosts_Quest>().Stage == 1) {
				return 0.085f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon
			);
		}
	}
	public class Spirit_Of_Christmas_Past : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.FairyCritterBlue];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Ghost);
			NPC.aiStyle = NPCAIStyleID.Bird;
			NPC.width = 18;
			NPC.height = 20;
			NPC.rarity += 2;
			NPC.lifeMax = 150;
			NPC.defense = 0;
			NPC.damage = 0;
			NPC.chaseable = false;
			AIType = NPCID.FairyCritterBlue;
			AnimationType = NPCID.FairyCritterBlue;
			Banner = Item.NPCtoBanner(NPCID.FairyCritterBlue);
		}
		public override bool PreAI() {
			if (NPC.aiAction == 1) {
				Tax_Collector_Ghosts_Quest.GetTime(out int hour, out _);
				if (hour != 1 && ++NPC.alpha > 255) NPC.active = false;
			}
			if (NPC.life == NPC.lifeMax) {
				if (NPC.ai[3] == -1000) {
					NPC.ai[3] = 0;
					NPC.aiAction = 1;
				}
				float oldValue = MathF.Sin(NPC.ai[3]);
				NPC.ai[3] += 0.02f;
				NPC.velocity.Y = (MathF.Sin(NPC.ai[3]) - oldValue) * 8;
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.type == NPCID.TaxCollector && other.Center.IsWithin(NPC.Center, 16 * 20)) {
						NPC.spriteDirection = NPC.direction = Math.Sign(other.Center.X - NPC.Center.X);
						break;
					}
				}
				return false;
			}
			NPC.chaseable = true;
			return true;
		}
		public override void AI() {
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Player.ZoneHallow && !NPC.AnyNPCs(Type) && ModContent.GetInstance<Tax_Collector_Ghosts_Quest>().Stage == 2) {
				return 0.085f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Graveyard
			);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 76);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 77);
			}
		}
	}
	public class Spirit_Of_Christmas_Present_Tax_Collector : ModNPC {
		public override string Texture => "Terraria/Images/NPC_" + NPCID.TaxCollector;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.TaxCollector];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.SpawnsWithCustomName[Type] = true;
			NPCID.Sets.ActsLikeTownNPC[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.TaxCollector);
			NPC.townNPC = false;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.alpha = 100;
			NPC.chaseable = false;
			AIType = NPCID.TaxCollector;
			AnimationType = NPCID.TaxCollector;
			Banner = Item.NPCtoBanner(NPCID.TaxCollector);
		}
		public override List<string> SetNPCNameList() {
			return [NPC.GetFirstNPCNameOrNull(NPCID.TaxCollector) ?? Lang.GetNPCNameValue(NPCID.TaxCollector)];
		}
		public override void AI() {
			int spiritID = ModContent.NPCType<Spirit_Of_Christmas_Present>();
			NPC.homeless = true;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type == spiritID) {
					NPC.homeTileX = (int)(other.position.X / 16) - other.direction * 4;
					NPC.homeTileY = (int)(other.position.Y / 16);
					return;
				}
			}
			if (++NPC.alpha >= 255) NPC.active = false;
		}
		public override bool CheckActive() {
			return Main.dayTime;
		}
	}
	public class Spirit_Of_Christmas_Present : ModNPC {
		public override string Texture => "Terraria/Images/TownNPCs/Shimmered/Santa_Default_Party";
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.SantaClaus];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.ActsLikeTownNPC[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.SantaClaus);
			NPC.lifeMax = 200;
			NPC.damage = 0;
			NPC.townNPC = false;
			NPC.friendly = false;
			NPC.rarity += 2;
			NPC.alpha = 100;
			NPC.chaseable = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath6;
			AIType = NPCID.SantaClaus;
			AnimationType = NPCID.SantaClaus;
			Banner = Item.NPCtoBanner(NPCID.SantaClaus);
		}
		public static bool FindTeleportPosition(out int bestX, out int bestY, out int minValue, out int maxValue) {
			int[] array = new int[200];
			int otherNPCs = 0;
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].active && Main.npc[i].townNPC && !(Main.npc[i].type is NPCID.OldMan or NPCID.TaxCollector)) {
					array[otherNPCs] = i;
					otherNPCs++;
				}
			}

			if (otherNPCs == 0) {
				bestX = bestY = minValue = maxValue = 0;
				return false;
			}

			int spawnNear = array[Main.rand.Next(otherNPCs)];
			bestX = Main.npc[spawnNear].homeTileX;
			bestY = Main.npc[spawnNear].homeTileY;
			minValue = bestX;
			maxValue = bestX;
			int x = bestX;
			int y = bestY;
			while (x > bestX - 10 && (WorldGen.SolidTile(x, y) || Main.tileSolidTop[Main.tile[x, y].TileType]) && (!Main.tile[x, y - 1].HasTile || !Main.tileSolid[Main.tile[x, y - 1].TileType] || Main.tileSolidTop[Main.tile[x, y - 1].TileType]) && (!Main.tile[x, y - 2].HasTile || !Main.tileSolid[Main.tile[x, y - 2].TileType] || Main.tileSolidTop[Main.tile[x, y - 2].TileType]) && (!Main.tile[x, y - 3].HasTile || !Main.tileSolid[Main.tile[x, y - 3].TileType] || Main.tileSolidTop[Main.tile[x, y - 3].TileType])) {
				minValue = x;
				x--;
			}

			for (int k = bestX; k < bestX + 10 && (WorldGen.SolidTile(k, y) || Main.tileSolidTop[Main.tile[k, y].TileType]) && (!Main.tile[k, y - 1].HasTile || !Main.tileSolid[Main.tile[k, y - 1].TileType] || Main.tileSolidTop[Main.tile[k, y - 1].TileType]) && (!Main.tile[k, y - 2].HasTile || !Main.tileSolid[Main.tile[k, y - 2].TileType] || Main.tileSolidTop[Main.tile[k, y - 2].TileType]) && (!Main.tile[k, y - 3].HasTile || !Main.tileSolid[Main.tile[k, y - 3].TileType] || Main.tileSolidTop[Main.tile[k, y - 3].TileType]); k++) {
				maxValue = k;
			}

			for (int l = 0; l < 30; l++) {
				int testX = Main.rand.Next(minValue, maxValue + 1);
				if (l < 20) {
					if (testX < bestX - 1 || testX > bestX + 1) {
						bestX = testX;
						break;
					}
				} else if (testX != bestX) {
					bestX = testX;
					break;
				}
			}
			return true;
		}
		public override bool PreAI() {
			NPC.homeless = true;
			if (NPC.ai[3] == -1000) {
				NPC.ai[3] = 0;
				NPC.aiAction = 1;
			}
			_ = NPC.position;
			if (NPC.justHit) {
				if (FindTeleportPosition(out int bestX, out int bestY, out int minValue, out int maxValue)) {
					ParticleOrchestraSettings particleSettings;
					int taxCollectorID = ModContent.NPCType<Spirit_Of_Christmas_Present_Tax_Collector>();
					foreach (NPC other in Main.ActiveNPCs) {
						if (other.type == taxCollectorID && other.Center.IsWithin(NPC.Center, 16 * 10)) {
							particleSettings = new() {
								PositionInWorld = other.Center
							};
							ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, particleSettings);
							other.Teleport(new(bestX * 16, bestY * 16 - 32), -1);
							ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, particleSettings);
							for (int l = 0; l < 30; l++) {
								int testX = Main.rand.Next(minValue, maxValue + 1);
								if (l < 20) {
									if (testX < bestX - 1 || testX > bestX + 1) {
										bestX = testX;
										break;
									}
								} else if (testX != bestX) {
									bestX = testX;
									break;
								}
							}
							break;
						}
					}
					particleSettings = new() {
						PositionInWorld = NPC.Center
					};
					ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, particleSettings);
					NPC.Teleport(new(bestX * 16, bestY * 16 - 32), -1);
					ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, particleSettings);
				}
			}
			NPC.chaseable = NPC.life != NPC.lifeMax;
			return true;
		}
		public override void AI() {
			if (NPC.aiAction == 1) {
				Tax_Collector_Ghosts_Quest.GetTime(out int hour, out _);
				if ((hour < 2 || Main.dayTime) && ++NPC.alpha > 255) NPC.active = false;
			}
			NPC.aiAction = 1;
		}
		public override bool CheckActive() {
			return Main.dayTime;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown && !NPC.AnyNPCs(Type) && ModContent.GetInstance<Tax_Collector_Ghosts_Quest>().Stage == 3) {
				return 0.1f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Christmas
			);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 20f, NPC.velocity, 1340);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 20f, NPC.velocity, 1340);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 34f, NPC.velocity, 1341);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 34f, NPC.velocity, 1341);
			}
		}
	}
	public class Spirit_Of_Christmas_Future_Tax_Collector : Spirit_Of_Christmas_Present_Tax_Collector {
		public override void AI() {
			int spiritID = ModContent.NPCType<Spirit_Of_Christmas_Future>();
			NPC.homeless = true;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type == spiritID && other.Center.IsWithin(NPC.Center, 16 * 20)) {
					NPC.homeTileX = (int)(other.position.X / 16) + other.direction * 4;
					NPC.homeTileY = (int)(other.position.Y / 16);
					if (!Main.dayTime) return;
					break;
				}
			}
			if (++NPC.alpha >= 255) NPC.active = false;
		}
	}
	public class Spirit_Of_Christmas_Future : ModNPC {
		public override string Texture => "Terraria/Images/NPC_" + NPCID.Wraith;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Wraith];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Wraith);
			NPC.rarity += 2;
			NPC.lifeMax = 500;
			NPC.defense = 40;
			NPC.damage = 0;
			NPC.chaseable = false;
			AIType = NPCID.Wraith;
			AnimationType = NPCID.Wraith;
			Banner = Item.NPCtoBanner(NPCID.Ghost);
		}
		public override bool PreAI() {
			float oldValue = MathF.Sin(NPC.ai[3]);
			NPC.ai[3] += 0.02f;
			NPC.velocity.Y = (MathF.Sin(NPC.ai[3]) - oldValue) * 8;
			NPC.velocity.X = 0;
			if (Main.dayTime && ++NPC.alpha >= 255) NPC.active = false;
			if (NPC.life == NPC.lifeMax) {
				int taxCollectorID = ModContent.NPCType<Spirit_Of_Christmas_Future_Tax_Collector>();
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.type == taxCollectorID && other.Center.IsWithin(NPC.Center, 16 * 20)) {
						NPC.spriteDirection = NPC.direction = Math.Sign(other.Center.X - NPC.Center.X);
						return false;
					}
				}
				NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.position.X, (int)NPC.position.Y, taxCollectorID);
			} else if (NPC.lastInteraction != 255 && Main.player[NPC.lastInteraction].active) {
				NPC.spriteDirection = NPC.direction = Math.Sign(Main.player[NPC.lastInteraction].Center.X - NPC.Center.X);
			}
			return false;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!Main.dayTime && spawnInfo.Player.ZoneGraveyard && !NPC.AnyNPCs(Type) && ModContent.GetInstance<Tax_Collector_Ghosts_Quest>().Stage == 4) {
				return 0.2f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Graveyard
			);
		}
	}
}
