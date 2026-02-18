using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Brine {
	public class King_Crab : Brine_Pool_NPC, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 0, 62, 60);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		static readonly AutoLoadingAsset<Texture2D> armTexture = typeof(King_Crab).GetDefaultTMLName() + "_Arm";
		static readonly AutoLoadingAsset<Texture2D> clawTexture = typeof(King_Crab).GetDefaultTMLName() + "_Claw";
		public float AttackTime => NPC.ai[1] / 30;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 5;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			TargetNPCTypes.Add(ModContent.NPCType<Mildew_Creeper>());
			TargetNPCTypes.Add(ModContent.NPCType<Carpalfish>());
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = -1;
			NPC.lifeMax = 500;
			NPC.defense = 26;
			NPC.damage = 38;
			NPC.width = 62;
			NPC.height = 68;
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
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 6)
			).WithOnSuccess(
				new LeadingConditionRule(new Conditions.IsHardmode()).WithOnSuccess(ItemDropRule.Food(ModContent.ItemType<Caeser_Salad>(), 40))
			));
		}
		public override bool CanTargetNPC(NPC other) => !OriginsSets.NPCs.TargetDummies[other.type] && CanHitNPC(other);
		public override bool CanTargetPlayer(Player player) => !player.invis;
		public int Frame {
			get {
				NPC.frame.Height = 358 / Main.npcFrameCount[NPC.type];
				return NPC.frame.Y / NPC.frame.Height;
			}
			set => NPC.frame.Y = NPC.frame.Height * value;
		}
		public override bool? CanFallThroughPlatforms() => targetIsBelow;
		bool targetIsBelow = false;
		public override void AI() {
			bool walkLeft = NPC.direction == -1;
			bool walkRight = NPC.direction == 1;
			bool hasBarrier = false;
			if (NPC.ai[1] <= 0) DoTargeting();
			bool foundTarget = TargetPos != default;
			if (NPC.ai[1] <= 0f) {
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
			if (AttackTime > 0 || (foundTarget && (NPC.Center + NPC.velocity - Vector2.UnitY * 16).WithinRange(TargetPos, 96))) {
				if (NPC.ai[1] <= 0) {
					if (--NPC.ai[1] < -5) {
						Vector2 dir = TargetPos - (NPC.Center - Vector2.UnitY * 16 + Vector2.UnitX * 32 * NPC.direction);
						NPC.aiAction = 0;
						dir.Y *= 1.2f;
						dir.X = Math.Abs(dir.X);
						if (dir.Y > dir.X) {
							NPC.aiAction = -1;
						} else if (dir.Y < -dir.X) {
							NPC.aiAction = 1;
						}
						NPC.ai[1] = 1;
					}
				} else {
					NPC.ai[1]++;
					if (AttackTime > 1) {
						NPC.ai[1] = 0;
					}
				}
			} else if (NPC.ai[1] < 0) {
				NPC.ai[1] = 0;
			}
			if (NPC.ai[1] > 0f) {
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
				if (!CollisionExt.CanHitRay(startPos, endPos) || !CollisionExt.CanHitRay(startPos + offset, endPos + offset) || !CollisionExt.CanHitRay(startPos - offset, endPos - offset)) {
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
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			bool af = OriginsModIntegrations.CheckAprilFools();
			float attackTime = AttackTime;
			switch ((int)(attackTime * 3)) {
				case 1:
				if (af) goto case 2;
				break;
				case 2:
				Rectangle clawHitbox = npcHitbox;
				clawHitbox.Inflate(-11, -13);
				clawHitbox.Y -= 16;
				clawHitbox.Offset((GeometryUtils.Vec2FromPolar(64, NPC.aiAction * -MathHelper.PiOver4) * new Vector2(NPC.direction, 1)).ToPoint());
				if (clawHitbox.Intersects(victimHitbox)) {
					if (af) immunityCooldownSlot = ImmunityCooldownID.DD2OgreKnockback;
					npcHitbox = clawHitbox;
					return true;
				}
				break;
			}
			if (npcHitbox.Intersects(victimHitbox)) damageMultiplier *= 0.5f;
			return true;
		}
		public override void FindFrame(int frameHeight) {

		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-4 * NPC.direction, -19).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/King_Crab2_Gore")
				);
				Gore.NewGoreDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-10 * NPC.direction, 13).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/King_Crab1_Gore")
				).Frame = new SpriteFrame(2, 1, 0, 0);
				Gore.NewGoreDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(10 * NPC.direction, 13).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/King_Crab1_Gore")
				).Frame = new SpriteFrame(2, 1, 1, 0);
				Gore.NewGoreDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-6 * NPC.direction, 13).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/King_Crab1_Gore")
				).Frame = new SpriteFrame(2, 1, 0, 0);
				Gore.NewGoreDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(6 * NPC.direction, 13).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/King_Crab1_Gore")
				).Frame = new SpriteFrame(2, 1, 1, 0);
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects inactiveArmEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
			SpriteEffects activeArmEffects = inactiveArmEffects ^ SpriteEffects.FlipVertically;
			Vector2 attachPoint = NPC.Center - screenPos;
			attachPoint.Y -= 15;
			attachPoint.Y += NPC.gfxOffY;
			float attackTime = AttackTime;
			Vector2[] inactiveArm = new Vector2[5];
			Vector2[] activeArm = new Vector2[5];
			inactiveArm[0] = attachPoint - new Vector2(7 * NPC.spriteDirection, 0);
			activeArm[0] = attachPoint + new Vector2(12 * NPC.spriteDirection, 0);
			FastRandom rand = new(NPC.frame.Y);
			float rot = ((rand.Next(120) % 4) * 0.05f + 0.3f) * -NPC.spriteDirection;
			inactiveArm[1] = inactiveArm[0] + new Vector2(0, 16).RotatedBy(rot * -2);
			inactiveArm[2] = inactiveArm[1] + new Vector2(0, 10).RotatedBy(rot * -1);
			inactiveArm[3] = inactiveArm[2] + new Vector2(0, 16).RotatedBy(rot * 0);
			inactiveArm[4] = inactiveArm[3] + new Vector2(0, 24).RotatedBy(rot * 1);
			int inactiveClawFrame = rand.Next(3);
			int activeClawFrame = rand.Next(3);
			if (attackTime <= 0) {
				rot = ((rand.Next(120) % 4) * 0.05f + 0.3f) * NPC.spriteDirection;
				activeArm[1] = activeArm[0] + new Vector2(0, 16).RotatedBy(rot * -2);
				activeArm[2] = activeArm[1] + new Vector2(0, 10).RotatedBy(rot * -1);
				activeArm[3] = activeArm[2] + new Vector2(0, 16).RotatedBy(rot * 0);
				activeArm[4] = activeArm[3] + new Vector2(0, 24).RotatedBy(rot * 1);
			} else {
				if (OriginsModIntegrations.CheckAprilFools()) {
					switch ((int)(attackTime * 3)) {
						case 0:
						activeArm[1] = activeArm[0] + new Vector2(0, 16).RotatedBy(0.25f);
						activeArm[2] = activeArm[1] + new Vector2(0, 10).RotatedBy(0.25f);
						activeArm[3] = activeArm[2] + new Vector2(0, 16).RotatedBy(0.25f);
						activeArm[4] = activeArm[3] + new Vector2(0, 24).RotatedBy(0.25f);
						break;
						default:
						activeArm[1] = activeArm[0] + new Vector2(16 * NPC.direction, 0).RotatedBy(rot * -2);
						activeArm[2] = activeArm[1] + new Vector2(10 * NPC.direction, 0).RotatedBy(rot * -1);
						activeArm[3] = activeArm[2] + new Vector2(16 * NPC.direction, 0).RotatedBy(rot * 0);
						activeArm[4] = activeArm[3] + new Vector2(24 * NPC.direction, 0).RotatedBy(rot * 1);
						break;
					}
				} else {
					rot = -MathHelper.PiOver4 * NPC.aiAction * NPC.spriteDirection;
					switch ((int)(attackTime * 3)) {
						case 0:
						activeArm[1] = activeArm[0] + new Vector2(0, 16).RotatedBy(0.15f * NPC.spriteDirection + rot * 0.2f);
						activeArm[2] = activeArm[1] + new Vector2(0, 10).RotatedBy(0.05f * NPC.spriteDirection + rot * 0.2f);
						activeArm[3] = activeArm[2] + new Vector2(0, 16).RotatedBy(-0.25f * NPC.spriteDirection + rot * 0.2f);
						activeArm[4] = activeArm[3] + new Vector2(0, 24).RotatedBy(-0.45f * NPC.spriteDirection + rot * 0.2f);
						activeClawFrame = 2;
						break;
						case 1:
						activeArm[1] = activeArm[0] + new Vector2(0, 16).RotatedBy(-0.35f * NPC.spriteDirection + rot * 0.7f);
						activeArm[2] = activeArm[1] + new Vector2(0, 10).RotatedBy(-0.45f * NPC.spriteDirection + rot * 0.7f);
						activeArm[3] = activeArm[2] + new Vector2(0, 16).RotatedBy(-0.55f * NPC.spriteDirection + rot * 0.7f);
						activeArm[4] = activeArm[3] + new Vector2(0, 24).RotatedBy(-0.65f * NPC.spriteDirection + rot * 0.7f);
						activeClawFrame = 1;
						break;
						default:
						activeArm[1] = activeArm[0] + new Vector2(16 * NPC.direction, 0).RotatedBy(rot);
						activeArm[2] = activeArm[1] + new Vector2(10 * NPC.direction, 0).RotatedBy(rot);
						activeArm[3] = activeArm[2] + new Vector2(16 * NPC.direction, 0).RotatedBy(rot);
						activeArm[4] = activeArm[3] + new Vector2(24 * NPC.direction, 0).RotatedBy(rot);
						activeClawFrame = 0;
						break;
					}
				}
			}
			void DrawArm(Vector2[] arm, int index, Rectangle frame, Texture2D texture, SpriteEffects flip) {
				spriteBatch.Draw(
					texture,
					arm[index],
					frame,
					drawColor,
					(arm[index + 1] - arm[index]).ToRotation(),
					new Vector2(4, frame.Height * 0.5f),
					1,
					flip,
				0);
			}
			DrawArm(inactiveArm, 0, new Rectangle(0, 6, 20, 12), armTexture, inactiveArmEffects);
			DrawArm(inactiveArm, 1, new Rectangle(22, 4, 18, 16), armTexture, inactiveArmEffects);
			DrawArm(inactiveArm, 2, new Rectangle(42, 2, 24, 18), armTexture, inactiveArmEffects);
			DrawArm(inactiveArm, 3, new Rectangle(0, 38 * inactiveClawFrame, 32, 32), clawTexture, inactiveArmEffects);

			DrawArm(activeArm, 0, new Rectangle(0, 6, 20, 12), armTexture, activeArmEffects);
			DrawArm(activeArm, 1, new Rectangle(22, 4, 18, 16), armTexture, activeArmEffects);
			DrawArm(activeArm, 2, new Rectangle(42, 2, 24, 18), armTexture, activeArmEffects);
			DrawArm(activeArm, 3, new Rectangle(0, 38 * activeClawFrame, 32, 32), clawTexture, activeArmEffects);
		}

		public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {

		}
		public class Arm {
			public Vector2 start;
			public Vector2 end;
			public void MoveByStart(Vector2 target) {
				Vector2 diff = target - start;
				start += diff;
				end += diff;
			}
			public void MoveByEnd(Vector2 target) {
				Vector2 diff = target - end;
				start += diff;
				end += diff;
			}
			public void DoIKMove(Vector2 target, float maxSpeed) {
				Vector2 diff = target - end;
				Vector2 newPos = end + diff.WithMaxLength(maxSpeed);
				float angleDiff = GeometryUtils.AngleDif((end - start).ToRotation(), (target - start).ToRotation(), out int dir);
				end = end.RotatedBy(angleDiff * dir, start);
				MoveByEnd(newPos);
			}
		}
	}
}
