using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class King_Crab : Brine_Pool_NPC, IMeleeCollisionDataNPC {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 5;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Vulture);
			NPC.aiStyle = -1;
			NPC.lifeMax = 500;
			NPC.defense = 26;
			NPC.damage = 67;
			NPC.width = 62;
			NPC.height = 68;
			NPC.catchItem = 0;
			NPC.friendly = false;
			NPC.HitSound = SoundID.Item127;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.45f;
			NPC.value = 500;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.EnemyRate(spawnInfo, Brine_Pool.SpawnRates.Crab);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 6));
		}
		public override bool CanTargetNPC(NPC npc) => CanHitNPC(npc);
		public override bool CanTargetPlayer(Player player) => !player.invis;
		public int Frame {
			get => NPC.frame.Y / NPC.frame.Height;
			set => NPC.frame.Y = NPC.frame.Height * value;
		}
		public override bool? CanFallThroughPlatforms() => targetIsBelow;
		bool targetIsBelow = false;
		public override void AI() {
			bool walkLeft = NPC.direction == -1;
			bool walkRight = NPC.direction == 1;
			bool hasBarrier = false;
			DoTargeting();
			bool foundTarget = TargetPos != default;
			if (NPC.ai[1] > 0f) {
				NPC.ai[1] -= 1f;
			} else {
				if (foundTarget) {
					targetIsBelow = TargetPos.Y > NPC.position.Y + NPC.height;
				}
				if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height)) {
					NPC.noTileCollide = false;
				}
				if (foundTarget && TargetPos.WithinRange(NPC.Center, 800)) {
					float xDirectionToTarget = TargetPos.X - NPC.Center.X;
					if (xDirectionToTarget < -10f) {
						walkLeft = true;
						walkRight = false;
					} else if (xDirectionToTarget > 10f) {
						walkRight = true;
						walkLeft = false;
					}
					if (TargetPos.Y < NPC.Center.Y - 100f && Math.Sign(xDirectionToTarget) != -Math.Sign(NPC.velocity.X) && Math.Abs(xDirectionToTarget) < 50 && NPC.velocity.Y == 0) {
						NPC.velocity.Y = -10f;
					}
				}
			}
			if (NPC.ai[1] != 0f) {
				walkLeft = false;
				walkRight = false;
			}
			NPC.rotation = 0f;
			float maxSpeed = 6f;
			float acceleration = 0.175f;
			float friction = 0.9f;
			if (walkLeft || walkRight) {
				const float lookahead = 16;
				Vector2 startPos = NPC.Left;
				Vector2 endPos = default;
				if (walkLeft) {
					startPos.X += 1;
					endPos.X -= lookahead;
				}
				if (walkRight) {
					startPos.X += NPC.width - 1;
					endPos.X += lookahead;
				}
				endPos += startPos;
				Vector2 offset = Vector2.UnitY * 16;
				if (!CollisionExtensions.CanHitRay(startPos, endPos) || !CollisionExtensions.CanHitRay(startPos + offset, endPos + offset) || !CollisionExtensions.CanHitRay(startPos - offset, endPos - offset)) {
					hasBarrier = true;
				}
			}
			if (NPC.velocity.Y != 0) {
				friction = 0.97f;
				acceleration *= 0.3f;
			}
			if (walkLeft) {
				if (NPC.velocity.X > -3.5) {
					NPC.velocity.X -= acceleration;
				} else {
					NPC.velocity.X -= acceleration * 0.25f;
				}
			} else if (walkRight) {
				if (NPC.velocity.X < 3.5) {
					NPC.velocity.X += acceleration;
				} else {
					NPC.velocity.X += acceleration * 0.25f;
				}
			} else if (NPC.velocity.Y == 0) {
				if (NPC.velocity.X >= -acceleration && NPC.velocity.X <= acceleration) {
					NPC.velocity.X = 0f;
				}
			}
			NPC.velocity.X *= friction;
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			if (NPC.velocity.Y == 0f) {
				if (hasBarrier) {
					if (NPC.localAI[1] == NPC.position.X) {
						(walkLeft, walkRight) = (walkRight, walkLeft);
					} else {
						int groundTileX = (int)(NPC.position.X + NPC.width * (walkRight ? 1 : 0)) / 16;
						int groundTileY = (int)(NPC.position.Y + NPC.height + 15) / 16;
						if (Framing.GetTileSafely(groundTileX, groundTileY).HasSolidTile()) {
							try {
								if (walkLeft) {
									groundTileX--;
								}
								if (walkRight) {
									groundTileX++;
								}
								groundTileX += (int)NPC.velocity.X;
								if (!WorldGen.SolidTile(groundTileX, groundTileY - 1) && !WorldGen.SolidTile(groundTileX, groundTileY - 2)) {
									NPC.velocity.Y = -5.1f;
								} else if (!WorldGen.SolidTile(groundTileX, groundTileY - 2)) {
									NPC.velocity.Y = -7.1f;
								} else if (WorldGen.SolidTile(groundTileX, groundTileY - 5)) {
									NPC.velocity.Y = -11.1f;
								} else if (WorldGen.SolidTile(groundTileX, groundTileY - 4)) {
									NPC.velocity.Y = -10.1f;
								} else {
									NPC.velocity.Y = -9.1f;
								}
							} catch {
								NPC.velocity.Y = -9.1f;
							}
							NPC.localAI[1] = NPC.position.X;
						}
					}
				}/* else if (hasHole && foundTarget && targetRect.Bottom <= NPC.position.Y + NPC.height) {
					NPC.velocity.Y = -9.1f;
				}*/
			}
			if (NPC.velocity.X > maxSpeed) {
				NPC.velocity.X = maxSpeed;
			}
			if (NPC.velocity.X < -maxSpeed) {
				NPC.velocity.X = -maxSpeed;
			}
			if (walkLeft != (NPC.direction == -1) || walkRight != (NPC.direction == 1)) {
				NPC.localAI[1] = 0;
			}
			if (walkLeft) {
				NPC.direction = -1;
			} else if (walkRight) {
				NPC.direction = 1;
			}

			NPC.rotation = 0f;
			if (NPC.velocity.Y >= 0f && NPC.velocity.Y <= 0.8) {
				if (NPC.position.X == NPC.oldPosition.X) {
					Frame = 0;
					NPC.frameCounter = 0;
				} else if (NPC.velocity.X < -0.8 || NPC.velocity.X > 0.8) {
					//NPC.frameCounter++;
					NPC.frameCounter += Math.Abs((int)NPC.velocity.X);
					if (NPC.frameCounter > 5) {
						Frame++;
						NPC.frameCounter = 0;
					}
					if (Frame >= Main.npcFrameCount[Type]) {
						Frame = 0;
					}
				} else {
					Frame = 0;
					NPC.frameCounter = 0;
				}
			} else {
				NPC.frameCounter = 0;
				Frame = 2;
			}
			NPC.velocity.Y += 0.4f;
			if (NPC.velocity.Y > 10f) {
				NPC.velocity.Y = 10f;
			}
			if (NPC.direction != 0) NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 2; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			
		}

		public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
			
		}
	}
}
