using Origins.Buffs;
using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Crimson {
	public class Cannihound : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 82, 50);
		public int AnimationFrames => 120;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		private Projectile projTarget;
		private const int jumpDst = 230;
		private const int jumpMinDst = 16 * 5;
		private const int healAmt = 20;
		public bool wasHit;
		public bool turning;
		public int turnDirection;
		public int turnTime;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 18;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			CrimsonGlobalNPC.NPCTypes.Add(Type);
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(Type, 0.04f);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 70;
			NPC.defense = 6;
			NPC.damage = 25;
			NPC.width = 54;
			NPC.height = 50;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.3f;
			NPC.value = 75;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY > Main.worldSurface + 50) return 0;
			if (!((spawnInfo.SpawnTileType == TileID.Crimtane && spawnInfo.Player.ZoneCrimson)
			|| spawnInfo.SpawnTileType == TileID.CrimsonGrass || spawnInfo.SpawnTileType == TileID.FleshIce
			|| spawnInfo.SpawnTileType == TileID.Crimstone || spawnInfo.SpawnTileType == TileID.Crimsand)) return 0;
			float chance = 0.6f;
			if (spawnInfo.Player.HasBuff<Cannihound_Lure_Debuff>()) chance *= 2;
			return (spawnInfo.Player.ZoneSnow ? 1.5f : 1) * chance;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson
			);
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			if (NPC.frame.Y / 50 != 12 && NPC.aiAction == 0) npcHitbox = default; // makes the NPC's attack hitbox 0, 0, 0, 0, preventing it from hitting anything, even things that aren't NPCs or Players
			if (NPC.ai[3] > 0 && npcHitbox.Intersects(victimHitbox)) damageMultiplier *= 1.1f;
			return true;
		}
		public override void UpdateLifeRegen(ref int damage) {
			if (NPC.ai[3] > 0) NPC.lifeRegen += 4;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fresh_Meat_Artifact>(), 40));
		}
		public override bool? CanFallThroughPlatforms() => NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override void AI() {
			NPC.TargetClosest(false);
			NPCAimedTarget target = NPC.GetTargetData();
			if (projTarget is not null && (!projTarget.active || projTarget.ModProjectile is not GoreProjectile)) projTarget = null;
			NPC.targetRect = projTarget?.Hitbox ?? target.Hitbox;

			// most ai styles use the ai variables, so we should treat them as uninitialized whenever the AI runs with one
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}
			switch (NPC.aiAction) {
				case 0:
				if (NPC.ai[1]-- <= 0 && NPC.collideY) {
					projTarget = null;
					if (NPC.GetLifePercent() < 1f || NPC.ai[2] > 0) {
						float nearestTarget = NPC.Center.DistanceSQ(target.Center) * 0.8f;
						foreach (Projectile proj in Main.ActiveProjectiles) {
							if (proj.ModProjectile is not GoreProjectile) continue;
							float newDist = NPC.Center.DistanceSQ(proj.Center);
							if (nearestTarget > newDist) {
								projTarget = proj;
								NPC.targetRect = proj.Hitbox;
								nearestTarget = newDist;
							}
						}
					}
					float dist = NPC.Center.DistanceSQ(NPC.targetRect.Center());
					if (dist is <= jumpDst * jumpDst and >= jumpMinDst * jumpMinDst && (NPC.HasValidTarget || projTarget is not null)) {
						NPC.aiAction = 1; // jump to target action
						NPC.ai[0] = 43; // time in ticks for the anim of the jump
						return;
					}
					NPC.TryJumpOverObstacles(NPC.direction, NPC.collideY);
					/*if (NPC.localAI[1] == NPC.position.X) {
						NPC.direction = -NPC.direction;
					} else if () {
						NPC.localAI[1] = NPC.position.X;
					}*/
				}
				break;
				case 1:
				if (--NPC.ai[0] == 15) {
					if (NPC.collideY) {
						Vector2 pos = NPC.targetRect.Top();
						Vector2 velocity;
						float speed = 15;
						if (GeometryUtils.AngleToTarget(pos - NPC.Center, speed, 0.2f, false) is float angle) {
							velocity = angle.ToRotationVector2() * speed;
						} else {
							float val = 0.70710678119f;
							velocity = new Vector2(val * NPC.direction, -val) * speed;
						}
						NPC.velocity = velocity;
						NPC.direction = Math.Sign(NPC.velocity.X);
					} else {
						NPC.ai[0]++;
					}
				}
				if (NPC.ai[0] <= 5 && (NPC.collideX || NPC.collideY || NPC.Center.Distance(NPC.targetRect.Center()) >= 230 || NPC.Hitbox.Intersects(NPC.targetRect))) {
					NPC.aiAction = 0;
					NPC.ai[1] = 60; // time in ticks before can jump again
				}
				break;
			}
			float distanceFromTarget = NPC.targetRect.Center().Clamp(NPC.Hitbox).Distance(NPC.Center.Clamp(NPC.targetRect));
			//Projectile.friendly = false;
			bool attac = distanceFromTarget < 16 || ((!NPC.collideY || NPC.localAI[2] >= 24) && NPC.localAI[2] <= 21);
			if (!attac) {
				const int prediction = 12;
				Rectangle projHitbox = NPC.Hitbox;
				Rectangle targHitbox = NPC.targetRect;
				Vector2 gravFactor = new(0, (prediction * (prediction + 1)) * 0.5f);
				projHitbox.Offset((NPC.velocity * prediction + NPC.gravity * gravFactor).ToPoint());
				if (projTarget is null) targHitbox.Offset((Main.player[NPC.target].velocity * prediction + Main.player[NPC.target].gravity * gravFactor).ToPoint());
				if (projHitbox.Intersects(NPC.targetRect)) {
					attac = true;
				}
			}
			float acceleration = 0.6f;
			if (attac) {
				if (++NPC.localAI[2] >= 21) {
					if (NPC.localAI[2] == 24 && projTarget is not null) {
						Rectangle meatRect = projTarget.Hitbox;
						projTarget.ModProjectile?.ModifyDamageHitbox(ref meatRect);
						if (NPC.Hitbox.Intersects(meatRect)) {
							int trueHealAmt = healAmt;
							if (NPC.life + healAmt > NPC.lifeMax) {
								trueHealAmt = NPC.lifeMax - NPC.life;
								if (NPC.ai[3] <= 0) NPC.ai[3] = 3 * 60; // time in ticks for the cannihound buffs
								else NPC.ai[3] += healAmt / 2;
							}
							NPC.life = Math.Min(NPC.life + trueHealAmt, NPC.lifeMax);
							NPC.HealEffect(trueHealAmt);
							NPC.netUpdate = true;
							projTarget.Kill();
						}
					}
					if (NPC.localAI[2] >= 27) NPC.localAI[2] = 0;
				}
				if (NPC.collideY) {
					acceleration = NPC.localAI[2] >= 18 ? 1.6f : 0.4f;
				}
			} else if (NPC.collideY || NPC.localAI[2] < 15) {
				NPC.localAI[2] = 0;
			}
			if (NPC.aiAction == 0) {
				NPC.ai[2]--;
				//NPC.aiStyle = NPCAIStyleID.Fighter;
			}

			bool targetInvalid = NPC.GetTargetData().Invalid;
			int currentMoveDirection = (NPC.velocity.X >= 0).ToDirectionInt();
			int targetMoveDirection = targetInvalid ? NPC.direction : (NPC.DirectionTo(NPC.targetRect.Center()).X >= 0).ToDirectionInt();

			if (NPC.aiAction != 0) acceleration = 0;
			if (targetInvalid) acceleration /= 3;
			if (currentMoveDirection != targetMoveDirection) {
				if (turnDirection != targetMoveDirection) {
					turnDirection = targetMoveDirection;
					turnTime = 0;
				}
				acceleration *= 0.25f;
			}
			if (!NPC.collideY) acceleration *= 0.25f;

			NPC.velocity.X += acceleration * targetMoveDirection;

			if (NPC.collideY || NPC.aiAction == 0) NPC.velocity.X *= 0.97f;
			if (currentMoveDirection == targetMoveDirection && NPC.aiAction == 0) NPC.velocity.X *= 0.93f;

			if (NPC.collideY) {
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}

			if (turnTime >= 6 * 2) NPC.direction = targetMoveDirection;
			NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			float speed = Math.Abs(NPC.velocity.X);
			if (speed > 5) speed = 2;
			else if (speed > 2) speed = 1;
			if (NPC.aiAction == 0) {
				if (NPC.ai[1] > 0) {
					NPC.frame.Height = 900 / Main.npcFrameCount[NPC.type];
					NPC.DoFrames(6, (NPC.frame.Y / NPC.frame.Height)..15);
					turnTime = 180;
				} else if (++turnTime < 6 * 2) {
					NPC.frame.Y = NPC.frame.Height * Math.Min(16 + turnTime / 6, Main.npcFrameCount[NPC.type]);
				} else {
					NPC.DoFrames(4 - (int)speed, 0..5);
				}
			} else if (NPC.ai[0] >= 16) {
				NPC.DoFrames(4, 6..15);
			}
			if (NPC.localAI[2] > 0) {
				NPC.frame.Y = NPC.frame.Height * ((int)(NPC.localAI[2] / 3f) + 6);
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					const float spread = 2;
					Projectile.NewProjectileDirect(
						NPC.GetSource_Death(),
						NPC.Center + new Vector2(22 * NPC.direction, -4),
						NPC.velocity + Main.rand.NextVector2Circular(spread, spread),
						ModContent.ProjectileType<Cannihound_Gore1>(),
						0, 0
					);
					Projectile.NewProjectileDirect(
						NPC.GetSource_Death(),
						NPC.Center + new Vector2(-10 * NPC.direction, 11),
						NPC.velocity + Main.rand.NextVector2Circular(spread, spread),
						ModContent.ProjectileType<Cannihound_Gore2>(),
						0, 0
					);
					Projectile.NewProjectileDirect(
						NPC.GetSource_Death(),
						NPC.Center + new Vector2(-18 * NPC.direction, 19),
						NPC.velocity + Main.rand.NextVector2Circular(spread, spread),
						ModContent.ProjectileType<Cannihound_Gore3>(),
						0, 0
					);
				}
				for (int i = Main.rand.Next(16, 25); i >= 0; i--) {
					Dust.NewDustDirect(
						NPC.position + Vector2.One * 2,
						NPC.width - 4,
						NPC.height - 4,
						DustID.Blood,
						NPC.velocity.X,
						NPC.velocity.Y
					);
				}
			}
		}
	}
	public class Cannihound_Lure_Debuff : ModBuff {
		public override string Texture => $"{nameof(Origins)}/Buffs/{nameof(Cannihound_Lure_Debuff)}";
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
		}
	}
}
