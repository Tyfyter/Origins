﻿using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Riven;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Measly_Moeba : Glowing_Mod_NPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Measly Moeba");
            Main.npcFrameCount[Type] = 4;
            SpawnModBiomes = new int[] {
                ModContent.GetInstance<Riven_Hive>().Type
            };
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
			NPC.alpha = 100;
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
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(""),
            });
        }
        public override void FindFrame(int frameHeight) {
		    NPC.spriteDirection = NPC.direction;
		    NPC.frameCounter += 0.5;
		    if (NPC.frameCounter >= 24.0){
			    NPC.frameCounter = 0.0;
		    }
		    NPC.frame.Y = 24 * (int)(NPC.frameCounter / 6.0);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {

        }
    }
}