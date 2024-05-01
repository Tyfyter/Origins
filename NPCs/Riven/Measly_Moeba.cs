using Microsoft.Xna.Framework;
using Origins.Items.Armor.Riven;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Measly_Moeba : ModNPC, IRivenEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.aiStyle = 0;
			NPC.lifeMax = 34;
			NPC.defense = 4;
			NPC.damage = 25;
			NPC.width = 20;
			NPC.height = 20;
			NPC.frame.Height = 22;
			NPC.alpha = 50;
			NPC.value = 20;
        }
		public override void AI() {
			if (NPC.direction == 0) {
				NPC.TargetClosest();
			}
			if (NPC.wet) {
				int centerTileX = (int)NPC.Center.X / 16;
				int centerTileY = (int)NPC.Center.Y / 16;
				if (Main.tile[centerTileX, centerTileY].TopSlope) {
					if (Main.tile[centerTileX, centerTileY].LeftSlope) {
						NPC.direction = -1;
						NPC.velocity.X = Math.Abs(NPC.velocity.X) * -0.9f;
					} else {
						NPC.direction = 1;
						NPC.velocity.X = Math.Abs(NPC.velocity.X) * 0.9f;
					}
				}
				if (NPC.collideX) {
					NPC.velocity.X *= -0.9f;
					NPC.direction *= -1;
				}
				if (NPC.collideY) {
					if (NPC.velocity.Y > 0f) {
						NPC.velocity.Y = NPC.velocity.Y * -0.9f;
						NPC.directionY = -1;
						NPC.ai[0] = -1f;
					} else if (NPC.velocity.Y < 0f) {
						NPC.velocity.Y = NPC.velocity.Y * -0.9f;
						NPC.directionY = 1;
						NPC.ai[0] = 1f;
					}
				}
				NPC.TargetClosest(faceTarget: false);
				Player target = Main.player[NPC.target];
				if (target.wet && !target.dead && Collision.CanHit(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height)) {
					NPC.localAI[2] = 1f;
					NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
					const float speed = 2f;
					Vector2 diff = target.Center - NPC.Center;
					diff *= speed / diff.Length();
					NPC.velocity = Vector2.Lerp(NPC.velocity, diff, 0.075f);
				} else {
					NPC.localAI[2] = 0f;
					NPC.velocity.X += NPC.direction * 0.02f;
					NPC.rotation = NPC.velocity.X * 0.4f;
					if (NPC.velocity.X < -1f || NPC.velocity.X > 1f) {
						NPC.velocity.X *= 0.95f;
					}
					if (NPC.ai[0] == -1f) {
						NPC.velocity.Y -= 0.01f;
						if (NPC.velocity.Y < -1f) {
							NPC.ai[0] = 1f;
						}
					} else {
						NPC.velocity.Y += 0.01f;
						if (NPC.velocity.Y > 1f) {
							NPC.ai[0] = -1f;
						}
					}
					if (Framing.GetTileSafely(centerTileX, centerTileY - 1).LiquidAmount > 128) {
						if (Framing.GetTileSafely(centerTileX, centerTileY + 1).HasTile) {
							NPC.ai[0] = -1f;
						} else if (Framing.GetTileSafely(centerTileX, centerTileY + 2).HasTile) {
							NPC.ai[0] = -1f;
						}
					} else {
						NPC.ai[0] = 1f;
					}
					if (NPC.velocity.Y > 1.2 || NPC.velocity.Y < -1.2) {
						NPC.velocity.Y *= 0.99f;
					}
				}
			} else {
				NPC.rotation += NPC.velocity.X * 0.075f;
				if (NPC.collideY) {
					NPC.rotation += NPC.velocity.X * 0.075f;
					NPC.velocity.X *= 0.98f;
					if (NPC.velocity.X > -0.01 && NPC.velocity.X < 0.01) {
						NPC.velocity.X = 0f;
					}
				}
				NPC.velocity.Y += 0.2f;
				if (NPC.velocity.Y > 10f) {
					NPC.velocity.Y -= 0.4f;
				}
				NPC.ai[0] = 1f;
			}
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Water) return 0f;
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Moeba;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("Small and nearly undetectable in its natural habitat of the pools of the Riven Hive, this variant of the Primordial Amoeba swims to facilitate the process of assimilation or digestion."),
			});
		}
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter += 0.5;
			if (NPC.frameCounter >= 24.0) {
				NPC.frameCounter = 0.0;
			}
			NPC.frame.Y = 24 * (int)(NPC.frameCounter / 6.0);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 2, 4));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
}
