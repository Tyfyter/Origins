using Origins.Buffs;
using Origins.Core.Structures;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riptide;
using Origins.Items.Other;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Medicine;
using Origins.Items.Other.Dyes;
using Origins.Items.Other.Fish;
using Origins.Items.Pets;
using Origins.Items.Tools;
using Origins.Items.Weapons.Melee;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Reflection;
using Origins.Tiles.Other;
using Origins.UI;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public static Dictionary<Guid, int> playersByGuid;
		public const float explosive_defense_factor = 2f;
		public static OriginPlayer LocalOriginPlayer { get; internal set; }
		public override void PreUpdateMovement() {
			Debugging.LogFirstRun(PreUpdateMovement);
			_ = Player.Hitbox;
			realControlUseItem = Player.controlUseItem;
			Origins.hurtCollisionCrimsonVine = false;
			if (riptideLegs && Player.wet) {
				Player.velocity *= 1.006f;
				Player.ignoreWater = true;
			}
			if (walledDebuff) {
				Player.velocity *= 0.4f;
				Player.velocity.Y *= 0.9f;
			}
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Latchkey_P>()] > 0) {
				Player.tongued = true;
			}
			bool otherDash = Player.dashType != 0;
			if (shineSpark && ((loversLeapDashTime <= 0 && shineSparkCharge > 0) || shineSparkDashTime > 0)) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const int shineSparkDuration = 90;
				float shineSparkSpeed = 16f;
				if (dashVase) shineSparkSpeed *= 1.2f;
				if (shineSparkDashTime > 0) {
					Player.velocity = shineSparkDashDirection * shineSparkDashSpeed;
					if (shineSparkDashDirection.X != 0 && onSlope) {
						shineSparkDashTime = 1;
						shineSparkCharge = 60;
					}
					if (collidingX || collidingY) {
						shineSparkDashTime = 1;
						Collision.HitTiles(Player.position, Player.velocity, Player.width, Player.height);
						if (!collidingX && shineSparkDashDirection.X != 0) {
							shineSparkCharge = 60;
						}
					}
					shineSparkDashTime--;
					dashDelay = 30;
					loversLeapDashTime = 0;
					Player.velocity.Y -= Player.gravity * Player.gravDir * 0.1f;
				} else {
					if (dashDirection != 0 || dashDirectionY != 0) {
						shineSparkDashTime = shineSparkDuration;
						shineSparkDashSpeed = shineSparkSpeed;
						Player.timeSinceLastDashStarted = 0;
						shineSparkDashDirection = new((Player.controlRight ? 1 : 0) - (Player.controlLeft ? 1 : 0), (Player.controlDown ? 1 : 0) - (Player.controlUp ? 1 : 0));
						shineSparkDashDirection.Normalize();
						if (collidingY && oldYSign > 0) Player.position.Y -= 1;
					}
				}
			} else if ((riptideSet && !Player.mount.Active) || riptideDashTime != 0) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const int riptideDashDuration = 12;
				float riptideDashSpeed = 9f;
				if (dashVase) riptideDashSpeed *= 1.2f;
				if (dashDirection != 0 && (Player.velocity.X * dashDirection < riptideDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					riptideDashTime = riptideDashDuration * dashDirection;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("riptide_dash"),
						Player.Center + new Vector2(Player.width * dashDirection, 0),
						new Vector2(dashDirection * riptideDashSpeed, 0),
						Riptide_Dash_P.ID,
						48,
						riptideDashSpeed + 3,
						Player.whoAmI
					);
				}
				if (riptideDashTime != 0) {
					Player.dashDelay = -1;
					Player.velocity.X = riptideDashSpeed * Math.Sign(riptideDashTime);
					riptideDashTime -= Math.Sign(riptideDashTime);
					dashDelay = 25;
					if (riptideDashTime == 0) Player.dashDelay = 25;
				}
			} else if (meatScribeItem is not null && meatDashCooldown <= 0) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const float meatDashTotalSpeed = 12f;
				float meatDashSpeed = meatDashTotalSpeed / Scribe_of_the_Meat_God_P.max_updates;
				if (dashVase) meatDashSpeed *= 1.2f;
				if (dashDirection != 0 && (Player.velocity.X * dashDirection < meatDashTotalSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("meat"),
						Player.Center + new Vector2(Player.width * dashDirection, 0),
						new Vector2(dashDirection * meatDashSpeed, 0),
						meatScribeItem.shoot,
						meatScribeItem.damage,
						meatDashSpeed * Scribe_of_the_Meat_God_P.max_updates + meatScribeItem.knockBack,
						Player.whoAmI
					);
					SoundEngine.PlaySound(SoundID.NPCDeath10.WithVolumeScale(0.75f), Player.position);
					dashDelay = Scribe_of_the_Meat_God_P.dash_duration + 6;
					meatDashCooldown = 120 + Scribe_of_the_Meat_God_P.dash_duration;
				}
			} else if (refactoringPieces && refactoringPiecesDashCooldown <= 0) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				float keyDashSpeed = 4;
				if (dashVase) keyDashSpeed *= 1.2f;
				if (dashDirection != 0) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Player.SpawnProjectile(
						Player.GetSource_Misc("refactor"),
						Player.Center + new Vector2(Player.width * dashDirection, 0),
						new Vector2(dashDirection * keyDashSpeed, 0),
						ModContent.ProjectileType<Latchkey_P>(),
						0,
						0
					);
					SoundEngine.PlaySound(Origins.Sounds.PowerUp.WithVolumeScale(0.75f), Player.position);
					dashDelay = 10 + 6;
					refactoringPiecesDashCooldown = 120;
				}
			} else if (shimmerShield && !Player.mount.Active) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const int shimmerDashDuration = 30;
				float shimmerDashSpeed = 9f;
				if (dashVase) shimmerDashSpeed *= 1.2f;
				if (dashDirection != 0 && (Player.velocity.X * dashDirection < shimmerDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					shimmerShieldDashTime = shimmerDashDuration * dashDirection;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
				}
				if (shimmerShieldDashTime != 0) {
					if (Math.Abs(shimmerShieldDashTime) > 15) {
						Player.dashDelay = -1;
						Player.velocity.X = shimmerDashSpeed * Math.Sign(shimmerShieldDashTime);
					} else {
						Player.dashDelay = 25;
					}
					Player.cShield = Shimmer_Dye.ShaderID;
					shimmerShieldDashTime -= Math.Sign(shimmerShieldDashTime);
					dashDelay = 25;
				}
			} else if (loversLeap) {
				const int loversLeapDuration = 6;
				float loversLeapSpeed = 12f;
				if (dashVase) loversLeapSpeed *= 1.2f;
				if (collidingX || collidingY) {
					Player.dashType = 0;
					Player.dashTime = 0;
					if ((dashDirection != 0 && (Player.velocity.X * dashDirection < loversLeapSpeed)) || (dashDirectionY != 0 && (Player.velocity.Y * dashDirectionY < loversLeapSpeed))) {
						//Player.dashDelay = -1;
						//Player.dash = 2;
						loversLeapDashTime = loversLeapDuration;
						loversLeapDashSpeed = loversLeapSpeed;
						Player.timeSinceLastDashStarted = 0;
						if (dashDirectionY == -1) {
							loversLeapDashDirection = 0;
						} else if (dashDirectionY == 1) {
							loversLeapDashDirection = 1;
						} else if (dashDirection == 1) {
							loversLeapDashDirection = 2;
						} else if (dashDirection == -1) {
							loversLeapDashDirection = 3;
						}
					}
				}
				if (loversLeapDashTime > 0) {
					if (loversLeapDashTime > 1) {
						switch (loversLeapDashDirection) {
							case 0:
							Player.velocity.Y = -loversLeapDashSpeed;
							break;
							case 1:
							Player.velocity.Y = loversLeapDashSpeed;
							break;
							case 2:
							Player.velocity.X = loversLeapDashSpeed;
							break;
							case 3:
							Player.velocity.X = -loversLeapDashSpeed;
							break;
						}
						loversLeapDashTime--;
						dashDelay = 30;
					}
					if ((loversLeapDashTime == 1 || loversLeapDashDirection == 2 || loversLeapDashDirection == 3) && loversLeapDashSpeed > 0) {
						bool bounce = false;
						bool canBounce = true;
						if (Math.Abs(Player.velocity.X) > Math.Abs(Player.velocity.Y)) {
							loversLeapDashDirection = Math.Sign(Player.velocity.X) == 1 ? 2 : 3;
						} else {
							loversLeapDashDirection = Math.Sign(Player.velocity.Y) == 1 ? 1 : 0;
						}
						Rectangle loversLeapHitbox = default;
						int hitDirection = 0;
						switch (loversLeapDashDirection) {
							case 0:
							canBounce = false;
							break;
							case 1:
							loversLeapHitbox = Player.Hitbox;
							loversLeapHitbox.Inflate(4, 4);
							loversLeapHitbox.Offset(0, 8);
							break;
							case 2:
							loversLeapHitbox = new Rectangle((int)(Player.position.X + Player.width), (int)(Player.position.Y - 4), 8, Player.height + 8);
							hitDirection = 1;
							break;
							case 3:
							loversLeapHitbox = new Rectangle((int)(Player.position.X - 8), (int)(Player.position.Y - 4), 8, Player.height + 8);
							hitDirection = -1;
							break;
						}
						if (canBounce) {
							for (int i = 0; i < Main.maxNPCs; i++) {
								NPC npc = Main.npc[i];
								if (npc.active && !npc.dontTakeDamage) {
									if (!npc.friendly || (npc.type == NPCID.Guide && Player.killGuide) || (npc.type == NPCID.Clothier && Player.killClothier)) {
										if (loversLeapHitbox.Intersects(npc.Hitbox)) {
											bounce = true;
											NPC.HitModifiers modifiers = npc.GetIncomingStrikeModifiers(loversLeapItem.DamageType, hitDirection);

											Player.ApplyBannerOffenseBuff(npc, ref modifiers);
											if (npc.life > 5) {
												Player.OnHit(npc.Center.X, npc.Center.Y, npc);
											}
											modifiers.ArmorPenetration += Player.GetWeaponArmorPenetration(loversLeapItem);
											CombinedHooks.ModifyPlayerHitNPCWithItem(Player, loversLeapItem, npc, ref modifiers);

											NPC.HitInfo strike = modifiers.ToHitInfo(Player.GetWeaponDamage(loversLeapItem), Main.rand.Next(100) < Player.GetWeaponCrit(loversLeapItem), Player.GetWeaponKnockback(loversLeapItem), damageVariation: true, Player.luck);
											NPCKillAttempt attempt = new(npc);
											int dmgDealt = npc.StrikeNPC(strike);

											CombinedHooks.OnPlayerHitNPCWithItem(Player, loversLeapItem, npc, in strike, dmgDealt);
											PlayerMethods.ApplyNPCOnHitEffects(Player, loversLeapItem, Item.GetDrawHitbox(loversLeapItem.type, Player), strike.SourceDamage, strike.Knockback, npc.whoAmI, strike.SourceDamage, dmgDealt);
											int bannerID = Item.NPCtoBanner(npc.BannerID());
											if (bannerID >= 0) {
												Player.lastCreatureHit = bannerID;
											}
											if (Main.netMode != NetmodeID.SinglePlayer) {
												NetMessage.SendStrikeNPC(npc, in strike);
											}
											if (Player.accDreamCatcher && !npc.HideStrikeDamage) {
												Player.addDPS(dmgDealt);
											}
											if (attempt.DidNPCDie()) {
												Player.OnKillNPC(ref attempt, loversLeapItem);
											}
											Player.GiveImmuneTimeForCollisionAttack(4);
										}
									}
								}
							}
							if (bounce) {
								loversLeapDashDirection ^= 1;
								loversLeapDashTime = 2;
								loversLeapDashSpeed = Math.Min(loversLeapDashSpeed - 0.5f, 9f);
							}
							switch (loversLeapDashDirection) {
								case 2:
								case 3:
								if (bounce) {
									Player.velocity.Y -= 4;
									loversLeapDashSpeed -= 2f;
								}
								break;
								case 0:
								case 1:
								if (collidingX || collidingY) {
									loversLeapDashTime = 0;
								}
								break;
							}
						}
					} else if(loversLeapDashTime == 1) {
						loversLeapDashTime = 0;
					}
				} else {
					otherDash = true;
				}
			}
			if (dashVase && !otherDash) {
				Player.dashTime = 0;
				const int vaseDashDuration = 12;
				const int vaseDashCooldown = 18;
				float vaseDashSpeed = 8f;
				if (Player.dashDelay <= 0 && dashDirection != 0 && (Player.velocity.X * dashDirection < vaseDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.dashDelay = vaseDashDuration + vaseDashCooldown;
					Player.timeSinceLastDashStarted = 0;
					vaseDashDirection = dashDirection;
					if (Player.velocity.Y * Player.gravDir > Player.gravity * Player.gravDir * 8) {
						Player.velocity.Y = Player.gravity * 8;
					}
				}
				if (Player.dashDelay > vaseDashCooldown && vaseDashDirection != 0) {
					if (Player.velocity.X * vaseDashDirection < vaseDashSpeed) Player.velocity.X = vaseDashSpeed * vaseDashDirection;
					Dust.NewDust(
						Player.position,
						Player.width,
						Player.height,
						DustID.YellowTorch,
						0,
						Player.velocity.Y
					);
				} else {
					vaseDashDirection = 0;
					dashVaseVisual = false;
				}
			} else {
				vaseDashDirection = 0;
				dashVaseVisual = false;
			}
			if (rebreather && Player.breath < Player.breathMax) {
				if (Player.breathCD == 0 || rebreatherCounting) {
					rebreatherCounting = true;
					const float maxCount = 8f;
					const float maxSpeed = 16f;
					float speed = Math.Min(Player.velocity.Length() / maxSpeed, 1);
					if ((rebreatherCount += speed) >= maxCount) {
						rebreatherCounting = false;
						rebreatherCount -= maxCount;
						Player.breath++;
					}
				}
			}
			if (hookTarget >= 0) {//ropeVel.HasValue&&
				Player.fallStart = (int)(Player.position.Y / 16f);
				Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_P.ID) {
					Vector2 diff = Player.Center - projectile.Center;
					Vector2 normDiff = diff.SafeNormalize(default);
					float dot = Vector2.Dot(normDiff, Player.velocity.SafeNormalize(default));
					Player.velocity = Vector2.Lerp(normDiff * -16, Player.velocity, 0.85f + dot * 0.1f);
					if (diff.LengthSquared() > 64) {
						Player.GoingDownWithGrapple = true;
					}
					Player.RefreshMovementAbilities();
				}
			}
			oldXSign = Math.Sign(Player.velocity.X);
			oldYSign = Math.Sign(Player.velocity.Y);
			//endCustomMovement:
			hookTarget = -1;
			/*int directionX = Math.Sign(Player.velocity.X);
			int directionY = Math.Sign(Player.velocity.Y);
			int vine = ModContent.TileType<Brineglow_Vine>();
			foreach (Point item in Collision.GetTilesIn(Player.TopLeft, Player.BottomRight)) {
				Tile tile = Framing.GetTileSafely(item);
				if (tile.HasTile && tile.TileType == vine) {
					ref short windSpeed = ref tile.Get<TileExtraVisualData>().TileFrameX;
					windSpeed = (short)Math.Clamp(windSpeed + Player.velocity.X, -128, 128);
				}
			}*/
			if (weakShimmer) Player.ignoreWater = true;
			onSlope = false;
		}
		public override void PreUpdate() {
			Debugging.LogFirstRun(PreUpdate);
			if (OriginConfig.Instance.Assimilation) {
				foreach (AssimilationInfo info in IterateAssimilation()) {
					if (info.Percent > 0) Player.AddBuff(info.Type.Type, 5);
				}
			}
			if (rivenWet) {
				Player.gravity = 0.25f;
			}
			ceilingRavel = false;
			if (ravel && spiderRavel) {
				if (collidingX) {
					Player.gravity = 0;
					Player.velocity.Y *= 0.9f;
					if (Player.controlUp) {
						Player.velocity.Y -= 0.35f;
					}
					if (Player.controlDown) {
						Player.velocity.Y += 0.35f;
					}
				} else {
					bool colliding = false;
					float halfSpiderWidth = Player.width / 2 - 1;
					float halfSpiderHeight = Player.height / 2 + 4;
					for (int i = -1; i < 2; i++) {
						Tile currentTile = Main.tile[(Player.Center - Player.velocity + new Vector2(halfSpiderWidth * i, -halfSpiderHeight)).ToTileCoordinates()];
						if (currentTile.HasTile && Main.tileSolid[currentTile.TileType] && !Main.tileSolidTop[currentTile.TileType]) {
							colliding = true;
							break;
						}
					}
					if (colliding) {
						ceilingRavel = Player.controlUp;
						spiderRavelTime = 10;
					}
					if (Player.controlDown) {
						spiderRavelTime = 0;
					}
					if (spiderRavelTime > 0 && Player.controlUp) {
						Player.gravity = 0f;
						Player.velocity.Y -= 0.35f;
						for (int i = -1; i < 2; i++) {
							Tile currentTile = Main.tile[(Player.Center - Player.velocity + new Vector2(9 * i, -30)).ToTileCoordinates()];
							if (currentTile.HasTile && Main.tileSolid[currentTile.TileType] && !Main.tileSolidTop[currentTile.TileType]) {
								Player.velocity.Y -= 1;
								break;
							}
						}
						Collision.StepUp(ref Player.position, ref Player.velocity, Player.width, Player.height, ref Player.stepSpeed, ref Player.gfxOffY, -1, true);
					}
				}
			}
			if (weakShimmer) {
				Player.maxFallSpeed = 10f;
				Player.gravity = Player.defaultGravity;
				Player.jumpHeight = 15;
				Player.jumpSpeed = 5.01f;
			}
			fullSendHorseshoeBonus = false;
			if (fullSend && Player.noFallDmg) {
				Player.noFallDmg = false;
				fullSendHorseshoeBonus = true;
				if (Player.fallStart * 16 > Player.position.Y) Player.fallStart = (int)(Player.position.Y / 16f);
			}
		}
		public override void PostUpdate() {
			Debugging.LogFirstRun(PostUpdate);
			heldProjectile = -1;
			if (rasterizedTime > 0) {
				Player.velocity = Vector2.Lerp(Player.velocity, Player.oldVelocity, rasterizedTime * 0.06f);
				Player.position = Vector2.Lerp(Player.position, Player.oldPosition, rasterizedTime * 0.06f);
			}
			Player.oldVelocity = Player.velocity;
			rivenWet = false;
			if (!weakShimmer && (Player.wet || WaterCollision(Player.position, Player.width, Player.height)) && !(Player.lavaWet || Player.honeyWet || Player.shimmerWet)) {
				if (Player.InModBiome<Riven_Hive>()) {
					rivenWet = true;
					/*if (GameModeData.ExpertMode) {
						int duration = 432;
						int targetTime = 1440;
						float targetSeverity = 0f;
					} else if (GameModeData.MasterMode) {
						int duration = 676;
						int targetTime = 1440;
						float targetSeverity = 0f;
					} else if (GameModeData.NormalMode) {
						int duration = 188;
						int targetTime = 1440;
						float targetSeverity = 0f;
					} else if (GameModeData.Creative) {
						int duration = 188;
						int targetTime = 1440;
						float targetSeverity = 0.08f;
					}*/
					InflictTorn(Player, 188, 750, 1f, true);
					Player.velocity *= 0.95f;
					GetAssimilation<Riven_Assimilation>().Percent += 0.001f; // This value x60 for every second, remember 100% is the max assimilation. This should be 6% every second resulting in 16.67 seconds of total time to play in Riven Water
				} else if (Player.InModBiome<Brine_Pool>()) {
					Player.AddBuff(Toxic_Shock_Debuff.ID, 300);
				}
			}

			if (shineSpark) {
				if (shineSparkDashTime > 0) {
					shineSparkCharge = 0;
				} else {
					const int max_shinespark_charge = 50;
					const int shinespark_trigger_charge = 60 * 2 * -1;
					int dir = shineSparkCharge > 0 ? 1 : -1;
					bool isCharging = Math.Abs(Player.velocity.X) > 7;
					if (isCharging) {
						if (collidingY) shineSparkCharge += dir;
						if (shineSparkCharge < shinespark_trigger_charge || shineSparkCharge > max_shinespark_charge) shineSparkCharge = max_shinespark_charge;
					} else if (shineSparkCharge != 0) {
						shineSparkCharge -= dir;
					}
				}
			} else {
				shineSparkDashTime = 0;
				shineSparkCharge = 0;
			}
			if (Player.controlJump) {
				if (Player.controlRight) {
					min = float.MaxValue;
					max = float.MinValue;
				}
				if (min > Player.velocity.Y) min = Player.velocity.Y;
				if (max < Player.velocity.Y) max = Player.velocity.Y;
			}
		}
		float min = float.MaxValue;
		float max = float.MinValue;
		public override void OnRespawn() {
			oldGravDir = Player.gravDir;
			if (hasProtOS) {
				Protomind.PlayRandomMessage(Protomind.QuoteType.Respawn, protOSQuoteCooldown, Player.Top);
			}
		}
		public override void UpdateDead() {
			weakShimmer = false;
			timeSinceLastDeath = -1;
			tornCurrentSeverity = 0;
			tornTarget = 0f;
			mojoFlaskChargesUsed = 0;

			foreach (AssimilationInfo info in IterateAssimilation()) {
				info.Percent = 0;
			}

			selfDamageRally = 0;
			blastSetCharge = 0;
			ownedLargeGems.Clear();
		}
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			base.ModifyMaxStats(out health, out mana);
			mana.Base += quantumInjectors * Quantum_Injector.mana_per_use;
			if (tornCurrentSeverity > 0) {
				health *= 1 - tornCurrentSeverity;
				if (tornCurrentSeverity >= 1 && Player.whoAmI == Main.myPlayer && !Player.dead) {
					mildewHealth = 0;
					Player.KillMe(PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Torn").SelectFrom(Player.name).ToNetworkText()),
						9999, 0
					);
				}
			}
			if (cryostenBody) {
				health *= 1.08f;
			}
		}
		public override void NaturalLifeRegen(ref float regen) {
			if (tornCurrentSeverity > 0 && tornCurrentSeverity < 1) {
				regen /= (1 - tornCurrentSeverity) * 0.85f + 0.15f;
			}
			if (pacemaker) {
				if (pacemakerActive.TrySet(pacemakerTime > Pacemaker.DisableRegenTime(Player)) && pacemakerActive) {
					SoundEngine.PlaySound(SoundID.Item117.WithPitch(1.5f), Player.position);
					SoundEngine.PlaySound(SoundID.Item121.WithPitch(2f).WithVolume(0.5f), Player.position);
				}
				if (pacemakerActive) {
					regen *= Pacemaker.RegenMult;
					Max(ref Player.lifeRegen, 0);
					Player.lifeRegen = Main.rand.RandomRound(Player.lifeRegen * Pacemaker.RegenMult);
					if (Player.palladiumRegen) Player.lifeRegenCount += Main.rand.RandomRound(4 * (Pacemaker.RegenMult - 1));
				} else {
					SoundEngine.PlaySound(SoundID.Zombie87.WithVolumeScale(0.3f).WithPitch(-0.72f), Player.position, _ => !pacemakerActive);
					Min(ref regen, 0);
					Min(ref Player.lifeRegen, 0);
					Min(ref Player.lifeRegenCount, 0);
					Player.palladiumRegen = false;
				}
			}
			if (mildewHeart && Player.statLife <= 0) {
				Min(ref regen, 0);
				Min(ref Player.lifeRegen, 0);
				Min(ref Player.lifeRegenCount, 0);
				Player.palladiumRegen = false;
			}
		}
		public override void PostUpdateBuffs() {
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.owner == Player.whoAmI && projectile.GetGlobalProjectile<OriginGlobalProj>().weakpointAnalyzerTarget.HasValue) {
					Player.ownedProjectileCounts[projectile.type]--;
				}
			}
			if (Player.whoAmI == Main.myPlayer) {
				foreach (Quest quest in Quest_Registry.Quests) {
					if (quest.PreUpdateInventoryEvent is not null) {
						quest.PreUpdateInventoryEvent();
					}
				}
			}
			if (MojoInjectionActive) Mojo_Injection.UpdateEffect(this);
			if (CrownJewelActive) Crown_Jewel.UpdateEffect(this);
			if (sendBuffs && Player.whoAmI == Main.myPlayer && !NetmodeActive.SinglePlayer) {
				NetMessage.SendData(MessageID.PlayerBuffs, number: Main.myPlayer);
			}
			sendBuffs = false;
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if (hasPotatOS) {
				Potato_Battery.PlayRandomMessage(Potato_Battery.QuoteType.Death, potatOSQuoteCooldown, Player.Top);
			}
			if (hasProtOS) {
				Protomind.PlayRandomMessage(Protomind.QuoteType.Death, protOSQuoteCooldown, Player.Top);
			}
			if (talkingPet != -1) {
				Projectile pet = Main.projectile[talkingPet];
				if (pet.type == Chew_Toy.projectileID) {
					Chee_Toy_Messages.Instance.PlayRandomMessage(Chee_Toy_Message_Types.Death, pet.Top);
				}
			}
			if (Player.difficulty == 0 || Player.difficulty == 3) {
				for (int i = 0; i < 59; i++) {
					if (Player.inventory[i].stack > 0 && ModLargeGem.GemTextures[Player.inventory[i].type] is not null) {
						int num = Item.NewItem(Player.GetSource_Death(), (int)Player.position.X, (int)Player.position.Y, Player.width, Player.height, Player.inventory[i].type);
						Main.item[num].netDefaults(Player.inventory[i].netID);
						Main.item[num].Prefix(Player.inventory[i].prefix);
						Main.item[num].stack = Player.inventory[i].stack;
						Main.item[num].velocity.Y = Main.rand.Next(-20, 1) * 0.2f;
						Main.item[num].velocity.X = Main.rand.Next(-20, 21) * 0.2f;
						Main.item[num].noGrabDelay = 100;
						Main.item[num].favorited = false;
						Main.item[num].newAndShiny = false;
						if (Main.netMode == NetmodeID.MultiplayerClient)
							NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num);

						Player.inventory[i].SetDefaults();
					}
				}
			}
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			releaseTriggerSetBonus = !controlTriggerSetBonus;
			controlTriggerSetBonus = Keybindings.TriggerSetBonus.Current;
			if (controlTriggerSetBonus && releaseTriggerSetBonus) {
				TriggerSetBonus();
			}
			if (Keybindings.UseMojoFlask.JustPressed && IterateAssimilation().Any(a => a.Percent > 0)) {
				if (Player.nonTorch == -1) Player.nonTorch = Player.selectedItem;

				for (int i = 0; i < Player.inventory.Length; i++) {
					if (Player.inventory[i]?.ModItem is Mojo_Flask) {
						Player.selectedItem = i;
						Player.controlUseItem = true;
					}
				}
			}
			if (Player.controlDown && Player.releaseDown) {
				doubleTapDown = doubleTapDownTimer < 15;
				doubleTapDownTimer = 0;
			} else doubleTapDown = false;
		}
		public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
			if (Player.InModBiome<Brine_Pool>()) {
				Player.adjWater = false;
			}
			List<Item> items = new();
			Dictionary<int, Item> substituteItems = new();
			int switchblade = ModContent.ItemType<Switchblade_Shortsword>();
			Queue<Item> medicines = [];
			HashSet<int> medicineTypes = [];
			for (int i = 0; i < Player.inventory.Length; i++) {
				Item currentItem = Player.inventory[i];
				if (currentItem.IsAir) continue;
				if (currentItem.type == switchblade) {
					substituteItems.Add(items.Count, currentItem);
					items.Add(new Item(ModContent.ItemType<Switchblade_Broadsword>(), currentItem.stack, currentItem.prefix));
				}
				if (currentItem.ModItem is MedicineBase and not Multimed && medicineTypes.Add(currentItem.type)) {
					medicines.Enqueue(currentItem);
					substituteItems.Add(items.Count, new Item(ModContent.ItemType<AnyDifferentMedicine>(), 1));
					items.Add(new Item(ModContent.ItemType<AnyDifferentMedicine>(), 1));
				}
			}
			itemConsumedCallback = (item, index) => {
				if (substituteItems.TryGetValue(index, out Item consumed)) {
					if (consumed.ModItem is AnyDifferentMedicine) {
						if (--medicines.Dequeue().stack <= 0) consumed.TurnToAir();
					} else {
						consumed.stack = item.stack;
						if (consumed.stack <= 0) consumed.TurnToAir();
					}
				}
			};
			return items;
		}
		public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) {
			if (item.prefix == ModContent.PrefixType<Imperfect_Prefix>()) return false;
			return true;
		}

		public override void SaveData(TagCompound tag) {
			if (eyndumCore is not null) {
				tag.Add("EyndumCore", eyndumCore.Value);
			}
			tag.Add("MimicSetSelection", mimicSetChoices);
			tag.Add("journalUnlocked", journalUnlocked);
			if (journalDye is not null) {
				tag.Add("JournalDye", journalDye);
			}
			if (journalText is not null) {
				tag.Add("JournalText", journalText);
			}
			if (unlockedJournalEntries is not null) {
				tag.Add("UnlockedJournalEntries", unlockedJournalEntries.ToList());
			}
			if (unreadJournalEntries is not null) {
				tag.Add("UnreadJournalEntries", unreadJournalEntries.ToList());
			}
			if (startedQuests is not null) {
				tag.Add("UnlockedQuests", startedQuests.ToList());
			}
			TagCompound questsTag = [];
			foreach (Quest quest in Quest_Registry.Quests) {
				if (!quest.SaveToWorld) {
					TagCompound questTag = [];
					quest.SaveData(questTag);
					if (questTag.Count > 0) {
						questsTag.Add(quest.FullName, questTag);
						if (!Main.gameMenu) Mod.Logger.Info($"Saving {quest.NameValue} to player with data: {questTag}");
					} else {
						if (!Main.gameMenu) Mod.Logger.Info($"Not saving {quest.NameValue}, no data to save");
					}
				}
			}
			if (questsTag.Count > 0) {
				tag.Add("Quests", questsTag);
			}
			tag.Add("TimeSinceLastDeath", timeSinceLastDeath);
			TagCompound assimilations = [];
			foreach (AssimilationInfo info in IterateAssimilation()) {
				assimilations[info.Type.FullName] = info.Percent;
			}
			tag.Add("Assimilations", assimilations);
			tag.Add("mojoInjection", mojoInjection);
			tag.Add("crownJewel", crownJewel);
			tag.Add("thePlantUnlocks", unlockedPlantModes.Select(mode => new ItemDefinition(mode)).Concat(unloadedPlantModes).ToList());
			tag.Add("GUID", guid.ToByteArray());
		}
		public override void LoadData(TagCompound tag) {
			if (tag.SafeGet<Item>("EyndumCore") is Item eyndumCoreItem) {
				eyndumCore = new Ref<Item>(eyndumCoreItem);
			}
			if (tag.SafeGet<int>("MimicSetSelection") is int mimicSetSelection) {
				mimicSetChoices = mimicSetSelection;
			}
			if (tag.SafeGet<Item>("JournalDye") is Item journalDyeItem) {
				journalDye = journalDyeItem;
			}
			if (tag.SafeGet<List<string>>("UnlockedJournalEntries") is List<string> journalEntries) {
				unlockedJournalEntries = journalEntries.ToHashSet();
			}
			if (tag.SafeGet<List<string>>("UnreadJournalEntries") is List<string> _unreadJournalEntries) {
				unreadJournalEntries = _unreadJournalEntries.ToHashSet();
			}
			if (tag.SafeGet<List<string>>("UnlockedQuests") is List<string> unlockedQuests) {
				startedQuests = unlockedQuests.ToHashSet();
			}
			if (tag.ContainsKey("journalUnlocked")) {
				journalUnlocked = tag.Get<bool>("journalUnlocked");
			}
			journalText = tag.SafeGet<List<string>>("JournalText") ?? journalText;
			questsTag = tag.SafeGet<TagCompound>("Quests");
			if (tag.SafeGet<int>("TimeSinceLastDeath") is int timeSinceLastDeath) {
				this.timeSinceLastDeath = timeSinceLastDeath;
			}
			if (tag.TryGet("Assimilations", out TagCompound assimilations)) {
				foreach (AssimilationInfo info in IterateAssimilation()) {
					info.Percent = assimilations.SafeGet<float>(info.Type.FullName);
				}
			}
			mojoInjection = tag.SafeGet<bool>("mojoInjection");
			crownJewel = tag.SafeGet<bool>("crownJewel");
			unlockedPlantModes.Clear();
			unloadedPlantModes.Clear();
			if (tag.TryGet("thePlantUnlocks", out List<ItemDefinition> plantUnlocks)) {
				for (int i = 0; i < plantUnlocks.Count; i++) {
					if (plantUnlocks[i].IsUnloaded) {
						unloadedPlantModes.Add(plantUnlocks[i]);
					} else {
						unlockedPlantModes.Add(plantUnlocks[i].Type);
					}
				}
			}
			if (tag.TryGet("GUID", out byte[] guidBytes)) {
				guid = new Guid(guidBytes, false);
			} else {
				guid = Guid.NewGuid();
			}
		}
		TagCompound questsTag;
		public override void OnEnterWorld() {
			questsTag ??= [];
			TagCompound worldQuestsTag = ModContent.GetInstance<OriginSystem>().questsTag ?? [];
			Origins.instance.Logger.Debug("player quests: " + questsTag.ToString());
			Origins.instance.Logger.Debug("world quests: " + worldQuestsTag.ToString());
			foreach (Quest quest in Quest_Registry.Quests) {
				if (!quest.SaveToWorld) {
					quest.LoadData(questsTag.SafeGet<TagCompound>(quest.FullName) ?? []);
				} else if (Main.netMode != NetmodeID.MultiplayerClient) {
					quest.LoadData(worldQuestsTag.SafeGet<TagCompound>(quest.FullName) ?? []);
				}
			}
			netInitialized = false;
			ResetLaserTag();
		}
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			FishingLoot.Pool.CatchFish(Player, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
		}
		public override void GetDyeTraderReward(List<int> rewardPool) {
			for (int i = 0; i < Dye_Item.dyeItems.Count; i++) {
				Dye_Item dyeItem = Dye_Item.dyeItems[i];
				if (dyeItem.AddToDyeTrader(Player)) rewardPool.Add(dyeItem.Type);
			}
		}
		public override bool CanUseItem(Item item) {
			if (ravel) return false;
			return true;
		}
		public override void SetControls() {
			Debugging.LogFirstRun(SetControls);
		}
		public override void PreUpdateBuffs() {
			Debugging.LogFirstRun(PreUpdateBuffs);
		}
		public override bool PreModifyLuck(ref float luck) {
			Debugging.LogFirstRun(PreModifyLuck);
			return base.PreModifyLuck(ref luck);
		}
		public override bool PreItemCheck() {
			Debugging.LogFirstRun(PreItemCheck);
			compositeFrontArmWasEnabled = Player.compositeFrontArm.enabled;
			if (weakShimmer) {
				Player.shimmering = false;
				Weak_Shimmer_Debuff.isUpdatingShimmeryThing = true;
			}
			collidingX = oldXSign != 0 && Player.velocity.X == 0;
			collidingY = oldYSign != 0 && Player.velocity.Y == 0;
			if (disableUseItem) {
				itemUseOldDirection = Player.direction;
				return false;
			}
			if (luckyHatSet && !Player.ItemAnimationActive && Player.HeldItem.ChangePlayerDirectionOnShoot && (Player.HeldItem.CountsAsClass(DamageClass.Ranged) || Player.HeldItem.CountsAsClass(DamageClasses.Explosive)) && luckyHatSetTime < 90) {
				Player.direction = itemUseOldDirection;
			}
			ItemChecking = true;
			if (Player.HeldItem.ModItem is C6_Jackhammer or Miter_Saw && Player.controlUseTile) {
				if (Player.ItemAnimationEndingOrEnded) {
					Player.direction = itemUseOldDirection;
				} else if (Player.altFunctionUse == 2) {
					Player.controlUseItem = true;
				}
			}
			itemUseOldDirection = Player.direction;
			bool goingToUseItem = Player.controlUseItem && Player.releaseUseItem && Player.itemAnimation == 0 && Player.HeldItem.useStyle != ItemUseStyleID.None;
			if (goingToUseItem && (Player.HeldItem.IsAir || !CombinedHooks.CanUseItem(Player, Player.HeldItem))) {
				goingToUseItem = true;
			}
			if (focusPotion) {
				if (goingToUseItem) {
					focusPotionThisUse = Player.CheckMana(Focus_Potion.GetManaCost(Player.HeldItem), true);
					Player.manaRegenDelay = (int)Player.maxRegenDelay;
				} else if (Player.ItemAnimationEndingOrEnded) {
					focusPotionThisUse = false;
				}
			} else {
				focusPotionThisUse = false;
			}
			if (goingToUseItem) {
				if (Player.HeldItem.ChangePlayerDirectionOnShoot && !LuckyHatSetActive) {
					Vector2 unitX = Vector2.UnitX.RotatedBy(Player.fullRotation);
					Vector2 shootDirection = Main.MouseWorld - Player.RotatedRelativePoint(Player.MountedCenter);
					if (shootDirection != Vector2.Zero) shootDirection.Normalize();
					if (Player.direction != (Vector2.Dot(unitX, shootDirection) > 0 ? 1 : -1) && (Player.HeldItem.CountsAsClass(DamageClass.Ranged) || Player.HeldItem.CountsAsClass(DamageClasses.Explosive))) {
						if (luckyHatSet && luckyHatSetTime > 0) {
							luckyHatSetTime += 30;
							if (LuckyHatSetActive) {
								SoundEngine.PlaySound(SoundID.Camera.WithPitchRange(0.6f, 1f), Player.Center);
								SoundEngine.PlaySound(SoundID.Coins.WithPitchRange(0.6f, 1f), Player.Center);
							}
						}
					}
				}
			}
			return true;
		}
		public override void PostItemCheck() {
			Debugging.LogFirstRun(PostItemCheck);
			ItemChecking = false;
			releaseAltUse = !Player.controlUseTile;
			Weak_Shimmer_Debuff.isUpdatingShimmeryThing = false;
		}
		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			if (Gelatin_Bloom_Brooch.GetNameIndex(Player.name) != -1) yield return new(ModContent.ItemType<Gelatin_Bloom_Brooch>());
		}
		public void InflictAssimilation<TAssimilation>(float assimilationAmount) where TAssimilation : AssimilationDebuff => InflictAssimilation((ushort)ModContent.GetInstance<TAssimilation>().AssimilationType, assimilationAmount);
		public void InflictAssimilation(ushort assimilationType, float assimilationAmount) {
			GetAssimilation(assimilationType).Percent += assimilationAmount;
			if (Main.netMode == NetmodeID.SinglePlayer || Player.whoAmI == Main.myPlayer) return;
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(Origins.NetMessageType.inflict_assimilation);
			packet.Write((byte)Player.whoAmI);
			packet.Write((ushort)assimilationType);
			packet.Write(assimilationAmount);
			packet.Send(Player.whoAmI, Main.myPlayer);
		}
		public void ResetLaserTag() {
			if (!Laser_Tag_Console.LaserTagGameActive) laserTagRespawnDelay = 0;
			laserTagVestActive = false;
			laserTagPoints = 0;
			laserTagHits = 0;
			laserTagHP = 0;
			Laser_Tag_Console.LaserTagTimeLeft = -1;
		}
	}
}
