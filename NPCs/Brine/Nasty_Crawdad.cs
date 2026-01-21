using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Ranged;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class Nasty_Crawdad : Brine_Pool_NPC, ICustomWikiStat {
		[field: CloneByReference]
		public HashSet<int> PredatorNPCTypes { get; private set; } = [];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 9;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			TargetNPCTypes.Add(ModContent.NPCType<Shotgunfish>());
			PredatorNPCTypes.Add(ModContent.NPCType<Shotgunfish>());
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Crawdad);
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 140;
			NPC.defense = 14;
			NPC.width = 48;
			NPC.height = 26;
			NPC.damage = 70;
			NPC.knockBackResist = 0.45f;
			NPC.value = 500;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.EnemyRate(spawnInfo, Brine_Pool.SpawnRates.Crawdad, true);
		}
		public override bool CanSpawnInPosition(int tileX, int tileY) => base.CanSpawnInPosition(tileX, tileY) && AnyMossNearSpawn(tileX, tileY);
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 2)
			).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Crawdaddys_Revenge>(), 40)
			).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Grenade_Lawnchair>(), 120)
			));
		}
		public override bool CanTargetNPC(NPC other) => !OriginsSets.NPCs.TargetDummies[other.type] && CanHitNPC(other);
		public override bool CanTargetPlayer(Player player) => !player.invis;
		public int Frame {
			get {
				NPC.frame.Height = 306 / Main.npcFrameCount[NPC.type];
				return NPC.frame.Y / NPC.frame.Height;
			}
			set => NPC.frame.Y = NPC.frame.Height * value;
		}
		public override bool? CanFallThroughPlatforms() => targetIsBelow;
		bool targetIsBelow = false;
		public float AttackTime => NPC.ai[1] / 21;
		public Rectangle SnapHitbox {
			get {
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Inflate((NPC.height - NPC.width) / 2, 0);
				hitbox.X += NPC.direction * 40;
				return hitbox;
			}
		}
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
					if (!TargetIsRipple && NPC.HasNPCTarget && PredatorNPCTypes.Contains(Main.npc[NPC.TranslatedTargetIndex].type)) {
						(walkRight, walkLeft) = (walkLeft, walkRight);
					}
					if (TargetPos.Y < NPC.Center.Y - 100f && Math.Sign(xDirectionToTarget) != -Math.Sign(NPC.velocity.X) && Math.Abs(xDirectionToTarget) < 50 && NPC.velocity.Y == 0) {
						NPC.velocity.Y = -10f;
					}
				}
			}
			if (AttackTime > 0 || (foundTarget && NPC.targetRect.Intersects(SnapHitbox))) {
				if (NPC.ai[1] <= 0) {
					if (--NPC.ai[1] < -2) {
						Vector2 dir = TargetPos - (NPC.Center - Vector2.UnitY * 16 + Vector2.UnitX * 32 * NPC.direction);
						NPC.aiAction = 0;
						dir.Y *= 1.2f;
						dir.X = Math.Abs(dir.X);
						if (dir.Y < dir.X && dir.Y > -dir.X) {
							NPC.ai[1] = 1;
						}
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
				Vector2 offset = Vector2.UnitY * 8;
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
			if (NPC.ai[1] > 0) {
				Frame = 6 + (int)(AttackTime * 3);
			} else if (NPC.velocity.Y >= 0f && NPC.velocity.Y <= 0.8) {
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
					if (Frame >= 6) {
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
			float attackTime = AttackTime;
			switch ((int)(attackTime * 3)) {
				case 2:
				Rectangle clawHitbox = SnapHitbox;
				if (clawHitbox.Intersects(victimHitbox)) {
					npcHitbox = clawHitbox;
					return true;
				}
				break;
			}
			if (npcHitbox.Intersects(victimHitbox)) damageMultiplier *= 0.5f;
			return true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-14 * NPC.direction, 3).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Nasty_Crawdad2_Gore")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-8 * NPC.direction, 5).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Nasty_Crawdad2_Gore")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(29 * NPC.direction, 3).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Nasty_Crawdad1_Gore")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(6 * NPC.direction, 0).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Nasty_Crawdad3_Gore")
				);
			}
		}
	}
}
