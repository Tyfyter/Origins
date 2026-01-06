using Origins.Dev;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	public class Royal_Gel_Global : GlobalItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
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
			Projectile.ContinuouslyUpdateDamageStats = true;
			//AIType = ProjectileID.BabySlime;
		}
		public override void AI() {
			DrawOffsetX = -Projectile.width / 2;
			DrawOriginOffsetY = (int)(Projectile.height * -0.65f);
			SlimeAI(Projectile);
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
		public static void SlimeAI(Projectile projectile, bool canFly = true) {
			Player owner = Main.player[projectile.owner];
			if (!owner.active) {
				projectile.active = false;
				return;
			}
			projectile.localAI[2] = -1;
			bool targetLeft = false;
			bool targetRight = false;
			bool ownerBelow = false;
			bool checkTileBlocked = false;
			const int centerOffset = 10;
			int restPointOffset = 40 * (projectile.minionPos + 1) * owner.direction;
			if (owner.position.X + owner.width / 2 < projectile.position.X + projectile.width / 2 - centerOffset + restPointOffset) {
				targetLeft = true;
			} else if (owner.position.X + owner.width / 2 > projectile.position.X + projectile.width / 2 + centerOffset + restPointOffset) {
				targetRight = true;
			}
			if (projectile.ai[0] == -1f) {
				targetLeft = false;
				targetRight = true;
			}
			if (projectile.ai[0] == -2f) {
				targetLeft = false;
				targetRight = false;
			}
			projectile.tileCollide = true;
			if (projectile.ai[1] == 0f && canFly) {//start flying
				int distToStartFlying = 500 + 40 * projectile.minionPos;
				if (projectile.localAI[0] > 0f) {
					distToStartFlying += 500;
				}
				if (projectile.localAI[0] > 0f) {
					distToStartFlying += 100;
				}
				if (owner.rocketDelay2 > 0) {
					projectile.ai[0] = 1f;
				}
				Vector2 center = projectile.Center;
				float xDist = owner.position.X + owner.width / 2 - center.X;
				float yDist = owner.position.Y + owner.height / 2 - center.Y;
				float distSQ = xDist * xDist + yDist * yDist;
				if (distSQ > 2000f * 2000f) {
					projectile.position.X = owner.position.X + owner.width / 2 - projectile.width / 2;
					projectile.position.Y = owner.position.Y + owner.height / 2 - projectile.height / 2;
				} else if (distSQ > distToStartFlying * distToStartFlying) {
					if (yDist > 0f && projectile.velocity.Y < 0f) {
						projectile.velocity.Y = 0f;
					}
					if (yDist < 0f && projectile.velocity.Y > 0f) {
						projectile.velocity.Y = 0f;
					}
					projectile.ai[0] = 1f;
				}
			}
			if (projectile.ai[0] != 0f) {//flying 
				if (projectile.ai[2] < 0) projectile.ai[2]++;
				const float acceleration = 0.2f;
				const int distToStopFlying = 200;
				projectile.tileCollide = false;
				Vector2 center = projectile.Center;
				float maxTargetDist = 700f;//uses taxicab distance
				bool foundPotentialTarget = false;
				int newTarget = -1;
				for (int i = 0; i < 200; i++) {
					if (!Main.npc[i].CanBeChasedBy(projectile)) {
						continue;
					}
					float targetCenterX = Main.npc[i].position.X + Main.npc[i].width / 2;
					float targetCenterY = Main.npc[i].position.Y + Main.npc[i].height / 2;
					if (Math.Abs(owner.position.X + owner.width / 2 - targetCenterX) + Math.Abs(owner.position.Y + owner.height / 2 - targetCenterY) < maxTargetDist) {
						if (Collision.CanHit(projectile.position, projectile.width, projectile.height, Main.npc[i].position, Main.npc[i].width, Main.npc[i].height)) {
							newTarget = i;
						}
						foundPotentialTarget = true;
						break;
					}
				}
				float xDist = owner.position.X + owner.width / 2 - center.X;
				xDist -= 40 * owner.direction;
				if (!foundPotentialTarget) {
					xDist -= 40 * projectile.minionPos * owner.direction;
				}
				if (foundPotentialTarget && newTarget >= 0) {
					projectile.ai[0] = 0f;
				}

				float yDist = owner.position.Y + owner.height / 2 - center.Y;
				float dist = (float)Math.Sqrt(xDist * xDist + yDist * yDist);
				if (dist < distToStopFlying && owner.velocity.Y == 0f && projectile.position.Y + projectile.height <= owner.position.Y + owner.height && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height)) {
					projectile.ai[0] = 0f;
					if (projectile.velocity.Y < -6f) {
						projectile.velocity.Y = -6f;
					}
				}
				if (dist < 60f) {
					xDist = projectile.velocity.X;
					yDist = projectile.velocity.Y;
				} else {
					dist = 10f / dist;
					xDist *= dist;
					yDist *= dist;
				}
				if (projectile.velocity.X < xDist) {
					projectile.velocity.X += acceleration;
					if (projectile.velocity.X < 0f) {
						projectile.velocity.X += acceleration * 1.5f;
					}
				}
				if (projectile.velocity.X > xDist) {
					projectile.velocity.X -= acceleration;
					if (projectile.velocity.X > 0f) {
						projectile.velocity.X -= acceleration * 1.5f;
					}
				}
				if (projectile.velocity.Y < yDist) {
					projectile.velocity.Y += acceleration;
					if (projectile.velocity.Y < 0f) {
						projectile.velocity.Y += acceleration * 1.5f;
					}
				}
				if (projectile.velocity.Y > yDist) {
					projectile.velocity.Y -= acceleration;
					if (projectile.velocity.Y > 0f) {
						projectile.velocity.Y -= acceleration * 1.5f;
					}
				}

				if (projectile.velocity.X > 0.5) {
					projectile.spriteDirection = -1;
				} else if (projectile.velocity.X < -0.5) {
					projectile.spriteDirection = 1;
				}
				projectile.frameCounter++;
				if (projectile.frameCounter > 6) {
					projectile.frame++;
					projectile.frameCounter = 0;
				}
				if (projectile.frame < 2 || projectile.frame > 5) {
					projectile.frame = 2;
				}
				projectile.rotation = projectile.velocity.X * 0.1f;
			} else {//not flying
				OriginExtensions.AngularSmoothing(ref projectile.rotation, 0, 0.075f);
				float restPosOffset = 40 * projectile.minionPos;
				projectile.localAI[0] -= 1f;
				if (projectile.localAI[0] < 0f) {
					projectile.localAI[0] = 0f;
				}
				if (projectile.ai[1] > 0f) {
					projectile.ai[1] -= 1f;
				} else {
					float targetX = projectile.position.X;
					float targetY = projectile.position.Y;
					float targetDist = 100000f;
					float throughWallDist = targetDist;
					int targetIndex = -1;
					void targetingAlgorithm(NPC target, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
						float x = target.Center.X;
						float y = target.Center.Y;
						if (target.CanBeChasedBy(projectile)) {
							float currentDist = Math.Abs(projectile.position.X + projectile.width / 2 - x) + Math.Abs(projectile.position.Y + projectile.height / 2 - y);
							if (currentDist < targetDist) {
								if (targetIndex == -1 && currentDist <= throughWallDist) {
									throughWallDist = currentDist;
									targetX = x;
									targetY = y;
								}
								if (isPriorityTarget || Collision.CanHit(projectile.position, projectile.width, projectile.height, target.position, target.width, target.height)) {
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
					projectile.localAI[2] = targetIndex;
					float throughWallPrepDistance = 300f;
					if (projectile.position.Y > Main.worldSurface * 16.0) {
						throughWallPrepDistance = 150f;
					}
					if (targetDist < throughWallPrepDistance + restPosOffset && targetIndex == -1) {
						float xDistFromTarget = targetX - (projectile.position.X + projectile.width / 2);
						if (xDistFromTarget < -5f) {
							targetLeft = true;
							targetRight = false;
						} else if (xDistFromTarget > 5f) {
							targetRight = true;
							targetLeft = false;
						}
					}
					if (targetIndex >= 0 && targetDist < 800f + restPosOffset) {
						projectile.friendly = true;
						projectile.localAI[0] = 60;
						float targetDiffX = targetX - (projectile.position.X + projectile.width / 2);
						if (targetDiffX < -10f) {
							targetLeft = true;
							targetRight = false;
						} else if (targetDiffX > 10f) {
							targetRight = true;
							targetLeft = false;
						}
						if (targetY < projectile.Center.Y - 100f && targetDiffX > -50f && targetDiffX < 50f && projectile.velocity.Y == 0f) {
							float targetDistY = Math.Abs(targetY - projectile.Center.Y);
							if (targetDistY < 120f) {
								projectile.velocity.Y = -10f;
							} else if (targetDistY < 210f) {
								projectile.velocity.Y = -13f;
							} else if (targetDistY < 270f) {
								projectile.velocity.Y = -15f;
							} else if (targetDistY < 310f) {
								projectile.velocity.Y = -17f;
							} else if (targetDistY < 380f) {
								projectile.velocity.Y = -18f;
							}
						}
					} else {
						projectile.friendly = false;
					}
				}

				if (projectile.ai[1] != 0f) {
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
					if (projectile.velocity.X > -3.5) {
						projectile.velocity.X -= acceleration;
					} else {
						projectile.velocity.X -= acceleration * 0.25f;
					}
				} else if (targetRight) {
					if (projectile.velocity.X < 3.5) {
						projectile.velocity.X += acceleration;
					} else {
						projectile.velocity.X += acceleration * 0.25f;
					}
				} else {
					projectile.velocity.X *= 0.9f;
					if (projectile.velocity.X >= 0f - acceleration && projectile.velocity.X <= acceleration) {
						projectile.velocity.X = 0f;
					}
				}
				if (targetLeft || targetRight) {
					int checkTileX = (int)(projectile.position.X + projectile.width / 2) / 16;
					int checkTileY = (int)(projectile.position.Y + projectile.height / 2) / 16;
					if (targetLeft) {
						checkTileX--;
					}
					if (targetRight) {
						checkTileX++;
					}
					checkTileX += (int)projectile.velocity.X;
					if (WorldGen.SolidTile(checkTileX, checkTileY)) {
						checkTileBlocked = true;
					}
				}
				if (owner.position.Y + owner.height - 8f > projectile.position.Y + projectile.height) {
					ownerBelow = true;
				}
				Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
				if (projectile.velocity.Y == 0f) {
					if (!ownerBelow && (projectile.velocity.X < 0f || projectile.velocity.X > 0f)) {
						int checkTileX = (int)(projectile.position.X + projectile.width / 2) / 16;
						int checkTileY = (int)(projectile.position.Y + projectile.height / 2) / 16 + 1;
						if (targetLeft) {
							checkTileX--;
						}
						if (targetRight) {
							checkTileX++;
						}
						WorldGen.SolidTile(checkTileX, checkTileY);
					}
					if (checkTileBlocked) {
						int centerTileX = (int)(projectile.position.X + projectile.width / 2) / 16;
						int centerTileY = (int)(projectile.position.Y + projectile.height) / 16;
						if (WorldGen.SolidTileAllowBottomSlope(centerTileX, centerTileY) || Main.tile[centerTileX, centerTileY].BlockType != BlockType.Solid) {
							try {
								centerTileX = (int)(projectile.position.X + projectile.width / 2) / 16;
								centerTileY = (int)(projectile.position.Y + projectile.height / 2) / 16;
								if (targetLeft) {
									centerTileX--;
								}
								if (targetRight) {
									centerTileX++;
								}
								centerTileX += (int)projectile.velocity.X;
								if (!WorldGen.SolidTile(centerTileX, centerTileY - 1) && !WorldGen.SolidTile(centerTileX, centerTileY - 2)) {
									projectile.velocity.Y = -5.1f;
								} else if (!WorldGen.SolidTile(centerTileX, centerTileY - 2)) {
									projectile.velocity.Y = -7.1f;
								} else if (WorldGen.SolidTile(centerTileX, centerTileY - 5)) {
									projectile.velocity.Y = -11.1f;
								} else if (WorldGen.SolidTile(centerTileX, centerTileY - 4)) {
									projectile.velocity.Y = -10.1f;
								} else {
									projectile.velocity.Y = -9.1f;
								}
							} catch {
								projectile.velocity.Y = -9.1f;
							}
						}
					} else if (targetLeft || targetRight) {
						projectile.velocity.Y -= 6f;
					}
				}
				projectile.velocity.X = MathHelper.Clamp(projectile.velocity.X, -maxXSpeed, maxXSpeed);
				if (projectile.velocity.X != 0f) {
					projectile.direction = Math.Sign(projectile.velocity.X);
				}
				if (projectile.velocity.X > acceleration && targetRight) {
					projectile.direction = 1;
				}
				if (projectile.velocity.X < -acceleration && targetLeft) {
					projectile.direction = -1;
				}
				projectile.spriteDirection = -projectile.direction;
				bool noHorizontalMovement = projectile.position.X - projectile.oldPosition.X == 0f;
				if (projectile.velocity.Y >= 0f && projectile.velocity.Y <= 0.8) {
					if (noHorizontalMovement) {
						projectile.frameCounter++;
					} else {
						projectile.frameCounter += 3;
					}
				} else {
					projectile.frameCounter += 5;
				}
				if (projectile.frameCounter >= 20) {
					projectile.frameCounter -= 20;
					projectile.frame++;
				}
				if (projectile.frame > 1) {
					projectile.frame = 0;
				}
				if (projectile.wet && owner.position.Y + owner.height < projectile.position.Y + projectile.height && projectile.localAI[0] == 0f) {
					if (projectile.velocity.Y > -4f) {
						projectile.velocity.Y -= 0.2f;
					}
					if (projectile.velocity.Y > 0f) {
						projectile.velocity.Y *= 0.95f;
					}
				} else {
					projectile.velocity.Y += 0.4f;
				}
				if (projectile.velocity.Y > 10f) {
					projectile.velocity.Y = 10f;
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
			Projectile.DamageType = DamageClass.Summon;
			Projectile.alpha = 255;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.penetrate = 3;
			Projectile.friendly = true;
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
