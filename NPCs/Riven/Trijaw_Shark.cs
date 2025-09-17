using Origins.Dev;
using Origins.Items.Armor.Riven;
using Origins.Items.Other.Consumables.Food;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Trijaw_Shark : Glowing_Mod_NPC, ICustomCollisionNPC, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, -6, 96, 40);
		public int AnimationFrames => 16;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public bool IsSandshark => true;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(28, 0),
				PortraitPositionXOverride = 0,
				PortraitPositionYOverride = 8
			};
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.aiStyle = NPCAIStyleID.Sand_Shark;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.8f;
			NPC.behindTiles = true;

			NPC.lifeMax = 450;
			NPC.defense = 23;
			NPC.damage = 56;
			NPC.width = 86;
			NPC.height = 32;
			NPC.frame.Height = 38;
			NPC.value = 400;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Riven_Hive_Ocean>().Type,
				ModContent.GetInstance<Riven_Hive_Desert>().Type
			];
			AnimationType = NPCID.SandsharkCrimson;
			NPC.localAI[1] = 4;
		}
		public override void AI() {
			(NPC.localAI[2], NPC.localAI[3]) = (NPC.localAI[3], NPC.localAI[2]);
			NPC.localAI[3] = 0;
			if (NPC.localAI[2] < 4 && Main.netMode != NetmodeID.MultiplayerClient && CheckSummon()) {
				NPC.NewNPCDirect(
					NPC.GetSource_FromAI(),
					NPC.Center,
					ModContent.NPCType<Trijaw_Shark_Feesh>(),
					start: NPC.whoAmI,
					ai3: NPC.whoAmI
				);
			}
		}
		bool CheckSummon(bool pay = true) {
			if (NPC.localAI[1] > 0) {
				if (pay) NPC.localAI[1]--;
				return true;
			}
			if (NPC.life > 20) {
				if (pay) {
					NPC.HitInfo hit = new() {
						Damage = 20
					};
					NPC.StrikeNPC(hit, fromNet: true, true);
					if (Main.netMode != NetmodeID.SinglePlayer)
						NetMessage.SendStrikeNPC(NPC, hit);
				}
				return true;
			}
			return false;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.SharkFin, 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Nachos, 30));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 16));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 25));
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Water) {
				if (!Sandstorm.Happening || !spawnInfo.Player.ZoneSandstorm || !TileID.Sets.Conversion.Sand[spawnInfo.SpawnTileType] || !NPC.Spawning_SandstoneCheck(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY)) {
					return 0f;
				}
			}
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Shark1;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange([
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Sandstorm
			]);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
		public void PreUpdateCollision() { }
		public void PostUpdateCollision() { }
	}
	public class Trijaw_Shark_Feesh : Glowing_Mod_NPC, ICustomCollisionNPC {
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public bool IsSandshark => true;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			NPCID.Sets.PositiveNPCTypesExcludedFromDeathTally[Type] = true;
		}
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.8f;
			NPC.behindTiles = true;
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 50;
			NPC.defense = 11;
			NPC.damage = 28;
			NPC.width = 24;
			NPC.height = 18;
			NPC.value = 0;
			NPC.waterMovementSpeed = 1f;
			this.CopyBanner<Trijaw_Shark>();
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Riven_Hive_Ocean>().Type
			];
		}
		public override void AI() {
			if (Main.npc.IndexInRange((int)NPC.ai[3]) && Main.npc[(int)NPC.ai[3]] is NPC { active: true } parent) {
				parent.localAI[3]++;
				const float grouping_factor = 0.002f;
				const float spreading_factor = 0.1f;
				const float sheeping_factor = 0.02f;
				const float control_weight = 20f;

				Vector2 swarmCenter = default;
				Vector2 magnetism = default;
				Vector2 swarmVelocity = default;
				float totalWeight = 0;

				Vector2 enemyDir = default;
				NPC.timeLeft = 300 + Main.rand.Next(60);
				Vector2 nearestParentPoint = NPC.Center.Clamp(parent.Hitbox) + parent.DirectionTo(parent.targetRect.Center()) * 32;
				swarmCenter = nearestParentPoint * control_weight;
				Vector2 dir = NPC.DirectionTo(nearestParentPoint);
				if (dir.HasNaNs()) dir = default;
				swarmVelocity = dir * 8 * control_weight;
				magnetism = dir * 0.25f;
				totalWeight += control_weight;
				const float i_must_bee_traveling_on_now = 8;
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.type != Type || other.ai[3] != NPC.ai[3]) continue;
					float distSQ = other.DistanceSQ(NPC.Center);

					swarmCenter += other.Center;
					swarmVelocity += other.velocity;
					totalWeight += 1;
					if (distSQ <= i_must_bee_traveling_on_now * i_must_bee_traveling_on_now) magnetism += (NPC.Center - other.Center) * spreading_factor;
					//float blockDist = CollisionExtensions.
				}

				if (totalWeight > 0) {
					NPC.velocity =
						(NPC.velocity +
						((swarmCenter / totalWeight) - NPC.Center) * grouping_factor +
						magnetism +
						(swarmVelocity / totalWeight) * sheeping_factor)
						.WithMaxLength(7)
					;
				}
				if (enemyDir != default) {
					Vector2 norm = NPC.velocity.SafeNormalize(default);
					float dot = Vector2.Dot(enemyDir, norm);
					if (dot < 0) dot *= -0.9f;
					NPC.velocity -= NPC.velocity * (1 - MathF.Pow(dot, 3)) * 0.05f;
				}

				NPC.spriteDirection = Math.Sign(NPC.velocity.X);
				NPC.rotation = NPC.velocity.X * 0.1f;
				NPC.DoFrames(5);
			} else {
				if (Main.netMode != NetmodeID.MultiplayerClient) NPC.StrikeInstantKill();
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
		public void PreUpdateCollision() { }
		public void PostUpdateCollision() { }
	}
}
