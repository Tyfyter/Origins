using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Projectiles.Weapons;
using System;
using System.Drawing;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using ThoriumMod.Items.Donate;
using ThoriumMod.Projectiles;
using static Fargowiltas.FargoSets;

namespace Origins.Items.Accessories {
	public class Royal_Gel_Global : GlobalItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.RoyalGel;
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.RoyalGel;
		public override void SetStaticDefaults() {
			ContentSamples.ItemsByType[ItemID.RoyalGel].GetPrefixCategories().AddRange([PrefixCategory.AnyWeapon, PrefixCategory.Magic]);
		}
		public override void SetDefaults(Item entity) {
			entity.DamageType = DamageClass.Summon;
			entity.damage = 11;
			entity.knockBack = 0.5f;
		}
		public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
			if (player.whoAmI != Main.myPlayer) return;
			int minionID = ModContent.ProjectileType<Spiked_Slime_Minion>();
			if (player.ownedProjectileCounts[minionID] <= 0) {
				Projectile.NewProjectile(player.GetSource_Accessory(item), player.MountedCenter, Vector2.Zero, minionID, player.GetWeaponDamage(item), player.GetWeaponKnockback(item), player.whoAmI);
			}
		}
		public override int ChoosePrefix(Item item, UnifiedRandom rand) {
			return OriginExtensions.AccessoryOrSpecialPrefix(item, rand, PrefixCategory.AnyWeapon, PrefixCategory.Magic);
		}
	}
	public class Spiked_Slime_Minion : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Main.projFrames[ProjectileID.BabySlime];
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BabySlime);
			Projectile.minionSlots = 0;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.aiStyle = 0;
			//AIType = ProjectileID.BabySlime;
		}
		public override void AI() {
			DrawOffsetX = -Projectile.width / 2;
			DrawOriginOffsetY = (int)(Projectile.height * -0.65f);
			SlimeAI();
			if (Projectile.frame >= 2) {
				return;
			}
			//DrawOffsetX = -Projectile.width / 2;
			int npcTarget = (int)Projectile.localAI[2];
			NPC target = default;
			if (npcTarget >= 0) {
				target = Main.npc[npcTarget];
				if (!target.active) Projectile.localAI[2] = -1;
				if (Projectile.ai[2] >= 0) {
					const float dist = 8 * 16;
					if (target.position.Y + target.height * 0.5f <= Projectile.position.Y + Projectile.height && Projectile.DistanceSQ(target.Center) < dist * dist) {
						ShootSpikes();
					}
				}
			}
			static bool IsPlatform(Tile tile) {
				return tile.HasTile && Main.tileSolidTop[tile.TileType];
			}
			float projectileBottom = Projectile.position.Y + Projectile.height;
			bool targetBelow = (target ?? (Entity)Main.player[Projectile.owner]).Bottom.Y > projectileBottom - (projectileBottom % 16);
			if (Projectile.tileCollide
				&& Projectile.velocity.Y > 0
				&& !targetBelow
				&& (IsPlatform(Framing.GetTileSafely((Projectile.BottomLeft + Vector2.UnitY).ToTileCoordinates()))
				|| IsPlatform(Framing.GetTileSafely((Projectile.BottomRight + Vector2.UnitY).ToTileCoordinates())))
			) {
				Projectile.velocity.Y = 0;
				float tileEmbedpos = projectileBottom;
				if ((int)tileEmbedpos / 16 == (int)(tileEmbedpos + 1) / 16) {
					Projectile.position.Y -= tileEmbedpos % 16;
				}
			}
			if (Projectile.ai[2] >= 0) {
				if ((Projectile.velocity.Y < -5.9f) && Projectile.ai[2] >= 0) {
					Projectile.ai[2] = 1;
				} else if (Projectile.ai[2] == 1 && Projectile.velocity.Y >= 0) {
					ShootSpikes();
				}
			} else Projectile.ai[2]++;
			if (Projectile.owner == Main.myPlayer && !Main.LocalPlayer.npcTypeNoAggro[1]) Projectile.Kill();
		}
		void SlimeAI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active) {
				Projectile.active = false;
				return;
			}
			Projectile.localAI[2] = -1;
			bool targetLeft = false;
			bool targetRight = false;
			bool ownerBelow = false;
			bool checkTileBlocked = false;
			const int centerOffset = 10;
			int restPointOffset = 40 * (Projectile.minionPos + 1) * owner.direction;
			if (owner.position.X + owner.width / 2 < Projectile.position.X + Projectile.width / 2 - centerOffset + restPointOffset) {
				targetLeft = true;
			} else if (owner.position.X + owner.width / 2 > Projectile.position.X + Projectile.width / 2 + centerOffset + restPointOffset) {
				targetRight = true;
			}
			if (Projectile.ai[0] == -1f) {
				targetLeft = false;
				targetRight = true;
			}
			if (Projectile.ai[0] == -2f) {
				targetLeft = false;
				targetRight = false;
			}
			Projectile.tileCollide = true;
			if (Projectile.ai[1] == 0f) {//start flying
				int distToStartFlying = 500 + 40 * Projectile.minionPos;
				if (Projectile.localAI[0] > 0f) {
					distToStartFlying += 500;
				}
				if (Projectile.localAI[0] > 0f) {
					distToStartFlying += 100;
				}
				if (owner.rocketDelay2 > 0) {
					Projectile.ai[0] = 1f;
				}
				Vector2 center = Projectile.Center;
				float xDist = owner.position.X + owner.width / 2 - center.X;
				float yDist = owner.position.Y + owner.height / 2 - center.Y;
				float distSQ = xDist * xDist + yDist * yDist;
				if (distSQ > 2000f * 2000f) {
					Projectile.position.X = owner.position.X + owner.width / 2 - Projectile.width / 2;
					Projectile.position.Y = owner.position.Y + owner.height / 2 - Projectile.height / 2;
				} else if (distSQ > distToStartFlying * distToStartFlying) {
					if (yDist > 0f && Projectile.velocity.Y < 0f) {
						Projectile.velocity.Y = 0f;
					}
					if (yDist < 0f && Projectile.velocity.Y > 0f) {
						Projectile.velocity.Y = 0f;
					}
					Projectile.ai[0] = 1f;
				}
			}
			if (Projectile.ai[0] != 0f) {//flying 
				if (Projectile.ai[2] < 0) Projectile.ai[2]++;
				const float acceleration = 0.2f;
				const int distToStopFlying = 200;
				Projectile.tileCollide = false;
				Vector2 center = Projectile.Center;
				float maxTargetDist = 700f;//uses taxicab distance
				bool foundPotentialTarget = false;
				int newTarget = -1;
				for (int i = 0; i < 200; i++) {
					if (!Main.npc[i].CanBeChasedBy(this)) {
						continue;
					}
					float targetCenterX = Main.npc[i].position.X + Main.npc[i].width / 2;
					float targetCenterY = Main.npc[i].position.Y + Main.npc[i].height / 2;
					if (Math.Abs(owner.position.X + owner.width / 2 - targetCenterX) + Math.Abs(owner.position.Y + owner.height / 2 - targetCenterY) < maxTargetDist) {
						if (Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, Main.npc[i].position, Main.npc[i].width, Main.npc[i].height)) {
							newTarget = i;
						}
						foundPotentialTarget = true;
						break;
					}
				}
				float xDist = owner.position.X + owner.width / 2 - center.X;
				xDist -= 40 * owner.direction;
				if (!foundPotentialTarget) {
					xDist -= 40 * Projectile.minionPos * owner.direction;
				}
				if (foundPotentialTarget && newTarget >= 0) {
					Projectile.ai[0] = 0f;
				}

				float yDist = owner.position.Y + owner.height / 2 - center.Y;
				float dist = (float)Math.Sqrt(xDist * xDist + yDist * yDist);
				if (dist < distToStopFlying && owner.velocity.Y == 0f && Projectile.position.Y + Projectile.height <= owner.position.Y + owner.height && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
					Projectile.ai[0] = 0f;
					if (Projectile.velocity.Y < -6f) {
						Projectile.velocity.Y = -6f;
					}
				}
				if (dist < 60f) {
					xDist = Projectile.velocity.X;
					yDist = Projectile.velocity.Y;
				} else {
					dist = 10f / dist;
					xDist *= dist;
					yDist *= dist;
				}
				if (Projectile.velocity.X < xDist) {
					Projectile.velocity.X += acceleration;
					if (Projectile.velocity.X < 0f) {
						Projectile.velocity.X += acceleration * 1.5f;
					}
				}
				if (Projectile.velocity.X > xDist) {
					Projectile.velocity.X -= acceleration;
					if (Projectile.velocity.X > 0f) {
						Projectile.velocity.X -= acceleration * 1.5f;
					}
				}
				if (Projectile.velocity.Y < yDist) {
					Projectile.velocity.Y += acceleration;
					if (Projectile.velocity.Y < 0f) {
						Projectile.velocity.Y += acceleration * 1.5f;
					}
				}
				if (Projectile.velocity.Y > yDist) {
					Projectile.velocity.Y -= acceleration;
					if (Projectile.velocity.Y > 0f) {
						Projectile.velocity.Y -= acceleration * 1.5f;
					}
				}

				if (Projectile.velocity.X > 0.5) {
					Projectile.spriteDirection = -1;
				} else if (Projectile.velocity.X < -0.5) {
					Projectile.spriteDirection = 1;
				}
				Projectile.frameCounter++;
				if (Projectile.frameCounter > 6) {
					Projectile.frame++;
					Projectile.frameCounter = 0;
				}
				if (Projectile.frame < 2 || Projectile.frame > 5) {
					Projectile.frame = 2;
				}
				Projectile.rotation = Projectile.velocity.X * 0.1f;
			} else {//not flying
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.075f);
				float restPosOffset = 40 * Projectile.minionPos;
				Projectile.localAI[0] -= 1f;
				if (Projectile.localAI[0] < 0f) {
					Projectile.localAI[0] = 0f;
				}
				if (Projectile.ai[1] > 0f) {
					Projectile.ai[1] -= 1f;
				} else {
					float targetX = Projectile.position.X;
					float targetY = Projectile.position.Y;
					float targetDist = 100000f;
					float throughWallDist = targetDist;
					int targetIndex = -1;
					void targetingAlgorithm(NPC target, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
						float x = target.Center.X;
						float y = target.Center.Y;
						if (target.CanBeChasedBy(this)) {
							float currentDist = Math.Abs(Projectile.position.X + Projectile.width / 2 - x) + Math.Abs(Projectile.position.Y + Projectile.height / 2 - y);
							if (currentDist < targetDist) {
								if (targetIndex == -1 && currentDist <= throughWallDist) {
									throughWallDist = currentDist;
									targetX = x;
									targetY = y;
								}
								if (isPriorityTarget || Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
									targetDist = currentDist;
									targetX = x;
									targetY = y;
									targetIndex = target.whoAmI;
									foundTarget = true;
								}
							}
						}
					}
					owner.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
					
					if (targetIndex == -1 && throughWallDist < targetDist) {
						targetDist = throughWallDist;
					}
					Projectile.localAI[2] = targetIndex;
					float throughWallPrepDistance = 300f;
					if (Projectile.position.Y > Main.worldSurface * 16.0) {
						throughWallPrepDistance = 150f;
					}
					if (targetDist < throughWallPrepDistance + restPosOffset && targetIndex == -1) {
						float xDistFromTarget = targetX - (Projectile.position.X + Projectile.width / 2);
						if (xDistFromTarget < -5f) {
							targetLeft = true;
							targetRight = false;
						} else if (xDistFromTarget > 5f) {
							targetRight = true;
							targetLeft = false;
						}
					}
					if (targetIndex >= 0 && targetDist < 800f + restPosOffset) {
						Projectile.friendly = true;
						Projectile.localAI[0] = 60;
						float targetDiffX = targetX - (Projectile.position.X + Projectile.width / 2);
						if (targetDiffX < -10f) {
							targetLeft = true;
							targetRight = false;
						} else if (targetDiffX > 10f) {
							targetRight = true;
							targetLeft = false;
						}
						if (targetY < Projectile.Center.Y - 100f && targetDiffX > -50f && targetDiffX < 50f && Projectile.velocity.Y == 0f) {
							float targetDistY = Math.Abs(targetY - Projectile.Center.Y);
							if (targetDistY < 120f) {
								Projectile.velocity.Y = -10f;
							} else if (targetDistY < 210f) {
								Projectile.velocity.Y = -13f;
							} else if (targetDistY < 270f) {
								Projectile.velocity.Y = -15f;
							} else if (targetDistY < 310f) {
								Projectile.velocity.Y = -17f;
							} else if (targetDistY < 380f) {
								Projectile.velocity.Y = -18f;
							}
						}
					} else {
						Projectile.friendly = false;
					}
				}

				if (Projectile.ai[1] != 0f) {
					targetLeft = false;
					targetRight = false;
				}

				float maxXSpeed = 6f;
				float acceleration = 0.2f;
				if (maxXSpeed < Math.Abs(owner.velocity.X) + Math.Abs(owner.velocity.Y)) {
					maxXSpeed = Math.Abs(owner.velocity.X) + Math.Abs(owner.velocity.Y);
					acceleration = 0.3f;
				}
				if (targetLeft) {
					if (Projectile.velocity.X > -3.5) {
						Projectile.velocity.X -= acceleration;
					} else {
						Projectile.velocity.X -= acceleration * 0.25f;
					}
				} else if (targetRight) {
					if (Projectile.velocity.X < 3.5) {
						Projectile.velocity.X += acceleration;
					} else {
						Projectile.velocity.X += acceleration * 0.25f;
					}
				} else {
					Projectile.velocity.X *= 0.9f;
					if (Projectile.velocity.X >= 0f - acceleration && Projectile.velocity.X <= acceleration) {
						Projectile.velocity.X = 0f;
					}
				}
				if (targetLeft || targetRight) {
					int checkTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
					int checkTileY = (int)(Projectile.position.Y + Projectile.height / 2) / 16;
					if (targetLeft) {
						checkTileX--;
					}
					if (targetRight) {
						checkTileX++;
					}
					checkTileX += (int)Projectile.velocity.X;
					if (WorldGen.SolidTile(checkTileX, checkTileY)) {
						checkTileBlocked = true;
					}
				}
				if (owner.position.Y + owner.height - 8f > Projectile.position.Y + Projectile.height) {
					ownerBelow = true;
				}
				Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
				if (Projectile.velocity.Y == 0f) {
					if (!ownerBelow && (Projectile.velocity.X < 0f || Projectile.velocity.X > 0f)) {
						int checkTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
						int checkTileY = (int)(Projectile.position.Y + Projectile.height / 2) / 16 + 1;
						if (targetLeft) {
							checkTileX--;
						}
						if (targetRight) {
							checkTileX++;
						}
						WorldGen.SolidTile(checkTileX, checkTileY);
					}
					if (checkTileBlocked) {
						int centerTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
						int centerTileY = (int)(Projectile.position.Y + Projectile.height) / 16;
						if (WorldGen.SolidTileAllowBottomSlope(centerTileX, centerTileY) || Main.tile[centerTileX, centerTileY].BlockType != BlockType.Solid) {
							try {
								centerTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
								centerTileY = (int)(Projectile.position.Y + Projectile.height / 2) / 16;
								if (targetLeft) {
									centerTileX--;
								}
								if (targetRight) {
									centerTileX++;
								}
								centerTileX += (int)Projectile.velocity.X;
								if (!WorldGen.SolidTile(centerTileX, centerTileY - 1) && !WorldGen.SolidTile(centerTileX, centerTileY - 2)) {
									Projectile.velocity.Y = -5.1f;
								} else if (!WorldGen.SolidTile(centerTileX, centerTileY - 2)) {
									Projectile.velocity.Y = -7.1f;
								} else if (WorldGen.SolidTile(centerTileX, centerTileY - 5)) {
									Projectile.velocity.Y = -11.1f;
								} else if (WorldGen.SolidTile(centerTileX, centerTileY - 4)) {
									Projectile.velocity.Y = -10.1f;
								} else {
									Projectile.velocity.Y = -9.1f;
								}
							} catch {
								Projectile.velocity.Y = -9.1f;
							}
						}
					} else if (targetLeft || targetRight) {
						Projectile.velocity.Y -= 6f;
					}
				}
				Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -maxXSpeed, maxXSpeed);
				if (Projectile.velocity.X != 0f) {
					Projectile.direction = Math.Sign(Projectile.velocity.X);
				}
				if (Projectile.velocity.X > acceleration && targetRight) {
					Projectile.direction = 1;
				}
				if (Projectile.velocity.X < -acceleration && targetLeft) {
					Projectile.direction = -1;
				}
				Projectile.spriteDirection = -Projectile.direction;
				bool noHorizontalMovement = Projectile.position.X - Projectile.oldPosition.X == 0f;
				if (Projectile.velocity.Y >= 0f && Projectile.velocity.Y <= 0.8) {
					if (noHorizontalMovement) {
						Projectile.frameCounter++;
					} else {
						Projectile.frameCounter += 3;
					}
				} else {
					Projectile.frameCounter += 5;
				}
				if (Projectile.frameCounter >= 20) {
					Projectile.frameCounter -= 20;
					Projectile.frame++;
				}
				if (Projectile.frame > 1) {
					Projectile.frame = 0;
				}
				if (Projectile.wet && owner.position.Y + owner.height < Projectile.position.Y + Projectile.height && Projectile.localAI[0] == 0f) {
					if (Projectile.velocity.Y > -4f) {
						Projectile.velocity.Y -= 0.2f;
					}
					if (Projectile.velocity.Y > 0f) {
						Projectile.velocity.Y *= 0.95f;
					}
				} else {
					Projectile.velocity.Y += 0.4f;
				}
				if (Projectile.velocity.Y > 10f) {
					Projectile.velocity.Y = 10f;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[2] >= 0) ShootSpikes();
		}
		public void ShootSpikes() {
			if (Projectile.owner == Main.myPlayer) {
				int spikeCount = Main.rand.Next(3, 7);
				for (int i = 0; i < spikeCount; i++) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						new Vector2(0, -5).RotatedBy(1 - (i / (float)(spikeCount - 1)) * 2),
						ModContent.ProjectileType<Spiked_Slime_Minion_Spike>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
				Projectile.ai[2] = -45;
			}
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item154, Projectile.Center);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = true;
			return true;
		}
		public override Color? GetAlpha(Color lightColor) {
			return lightColor * 0.8f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
	}
	public class Spiked_Slime_Minion_Spike : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Main.projFrames[ProjectileID.SpikedSlimeSpike];
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpikedSlimeSpike);
			Projectile.aiStyle = 1;
			Projectile.penetrate = 3;
			Projectile.npcProj = false;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			//AIType = ProjectileID.SpikedSlimeSpike;
		}
		public override void AI() {
			if (Projectile.ai[0] >= 5f) {
				Projectile.ai[0] = 5f;
				Projectile.velocity.Y += 0.15f;
			}

			Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity,
				Projectile.width,
				Projectile.height,
				DustID.TintableDust,
				0f,
				0f,
				50,
				new Color(43, 185, 255, 150),
				1.0f
			);
			dust.velocity *= 0.3f;
			dust.velocity += Projectile.velocity * 0.3f;
			dust.noGravity = true;
			Projectile.alpha -= 50;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
	}
}
