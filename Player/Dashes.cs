using CalamityMod.Enums;
using Origins.Core;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riptide;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Dyes;
using Origins.Reflection;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public record class Dash_Action(Player Player, int DashDirection, int DashDirectionY) : SyncedAction {
		public Dash_Action() : this(default, default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadByte()],
			DashDirection = reader.ReadSByte(),
			DashDirectionY = reader.ReadSByte(),
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Player.whoAmI);
			writer.Write((sbyte)DashDirection);
			writer.Write((sbyte)DashDirectionY);
		}
		protected override void Perform() {
			bool otherDash = Player.dashType != 0;
			OriginPlayer originPlayer = Player.OriginPlayer();
			if (originPlayer.shineSpark && ((originPlayer.loversLeapDashTime <= 0 && originPlayer.shineSparkCharge > 0) || originPlayer.shineSparkDashTime > 0)) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const int shineSparkDuration = 90;
				float shineSparkSpeed = 16f;
				if (originPlayer.dashVase) shineSparkSpeed *= 1.2f;
				if (originPlayer.shineSparkDashTime > 0) {
					Player.velocity = originPlayer.shineSparkDashDirection * originPlayer.shineSparkDashSpeed;
					if (originPlayer.shineSparkDashDirection.X != 0 && originPlayer.onSlope) {
						originPlayer.shineSparkDashTime = 1;
						originPlayer.shineSparkCharge = 60;
					}
					if (originPlayer.collidingX || originPlayer.collidingY) {
						originPlayer.shineSparkDashTime = 1;
						Collision.HitTiles(Player.position, Player.velocity, Player.width, Player.height);
						if (!originPlayer.collidingX && originPlayer.shineSparkDashDirection.X != 0) {
							originPlayer.shineSparkCharge = 60;
						}
					}
					originPlayer.shineSparkDashTime--;
					originPlayer.dashDelay = 30;
					originPlayer.loversLeapDashTime = 0;
					Player.velocity.Y -= Player.gravity * Player.gravDir * 0.1f;
				} else {
					if (DashDirection != 0 || DashDirectionY != 0) {
						originPlayer.shineSparkDashTime = shineSparkDuration;
						originPlayer.shineSparkDashSpeed = shineSparkSpeed;
						Player.timeSinceLastDashStarted = 0;
						originPlayer.shineSparkDashDirection = new((Player.controlRight ? 1 : 0) - (Player.controlLeft ? 1 : 0), (Player.controlDown ? 1 : 0) - (Player.controlUp ? 1 : 0));
						originPlayer.shineSparkDashDirection.Normalize();
						if (originPlayer.collidingY && originPlayer.oldYSign > 0) Player.position.Y -= 1;
					}
				}
			} else if ((originPlayer.riptideSet && !Player.mount.Active) || originPlayer.riptideDashTime != 0) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const int riptideDashDuration = 12;
				float riptideDashSpeed = 9f;
				if (originPlayer.dashVase) riptideDashSpeed *= 1.2f;
				if (DashDirection != 0 && (Player.velocity.X * DashDirection < riptideDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					originPlayer.riptideDashTime = riptideDashDuration * DashDirection;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("riptide_dash"),
						Player.Center + new Vector2(Player.width * DashDirection, 0),
						new Vector2(DashDirection * riptideDashSpeed, 0),
						Riptide_Dash_P.ID,
						48,
						riptideDashSpeed + 3,
						Player.whoAmI
					);
				}
				if (originPlayer.riptideDashTime != 0) {
					Player.dashDelay = -1;
					Player.velocity.X = riptideDashSpeed * Math.Sign(originPlayer.riptideDashTime);
					originPlayer.riptideDashTime -= Math.Sign(originPlayer.riptideDashTime);
					originPlayer.dashDelay = 25;
					if (originPlayer.riptideDashTime == 0) Player.dashDelay = 25;
				}
			} else if (originPlayer.meatScribeItem is not null && originPlayer.meatDashCooldown <= 0) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const float meatDashTotalSpeed = 12f;
				float meatDashSpeed = meatDashTotalSpeed / Scribe_of_the_Meat_God_P.max_updates;
				if (originPlayer.dashVase) meatDashSpeed *= 1.2f;
				if (DashDirection != 0 && (Player.velocity.X * DashDirection < meatDashTotalSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("meat"),
						Player.Center + new Vector2(Player.width * DashDirection, 0),
						new Vector2(DashDirection * meatDashSpeed, 0),
						originPlayer.meatScribeItem.shoot,
						originPlayer.meatScribeItem.damage,
						meatDashSpeed * Scribe_of_the_Meat_God_P.max_updates + originPlayer.meatScribeItem.knockBack,
						Player.whoAmI
					);
					SoundEngine.PlaySound(SoundID.NPCDeath10.WithVolumeScale(0.75f), Player.position);
					originPlayer.dashDelay = Scribe_of_the_Meat_God_P.dash_duration + 6;
					originPlayer.meatDashCooldown = 120 + Scribe_of_the_Meat_God_P.dash_duration;
				}
			} else if (originPlayer.refactoringPieces && originPlayer.refactoringPiecesDashCooldown <= 0) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				float keyDashSpeed = 4;
				if (originPlayer.dashVase) keyDashSpeed *= 1.2f;
				if (DashDirection != 0) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Player.SpawnProjectile(
						Player.GetSource_Misc("refactor"),
						Player.Center + new Vector2(Player.width * DashDirection, 0),
						new Vector2(DashDirection * keyDashSpeed, 0),
						ModContent.ProjectileType<Latchkey_P>(),
						0,
						0
					);
					SoundEngine.PlaySound(Origins.Sounds.PowerUp.WithVolumeScale(0.75f), Player.position);
					originPlayer.dashDelay = 10 + 6;
					originPlayer.refactoringPiecesDashCooldown = 120;
				}
			} else if (originPlayer.shimmerShield && !Player.mount.Active) {
				otherDash = true;
				Player.dashType = 0;
				Player.dashTime = 0;
				const int shimmerDashDuration = 30;
				float shimmerDashSpeed = 9f;
				if (originPlayer.dashVase) shimmerDashSpeed *= 1.2f;
				if (DashDirection != 0 && (Player.velocity.X * DashDirection < shimmerDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					originPlayer.shimmerShieldDashTime = shimmerDashDuration * DashDirection;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
				}
				if (originPlayer.shimmerShieldDashTime != 0) {
					if (Math.Abs(originPlayer.shimmerShieldDashTime) > 15) {
						Player.dashDelay = -1;
						Player.velocity.X = shimmerDashSpeed * Math.Sign(originPlayer.shimmerShieldDashTime);
					} else {
						Player.dashDelay = 25;
					}
					Player.cShield = Shimmer_Dye.ShaderID;
					originPlayer.shimmerShieldDashTime -= Math.Sign(originPlayer.shimmerShieldDashTime);
					originPlayer.dashDelay = 25;
				}
			} else if (originPlayer.loversLeap) {
				const int loversLeapDuration = 6;
				float loversLeapSpeed = 12f;
				if (originPlayer.dashVase) loversLeapSpeed *= 1.2f;
				if (originPlayer.collidingX || originPlayer.collidingY) {
					Player.dashType = 0;
					Player.dashTime = 0;
					if ((DashDirection != 0 && (Player.velocity.X * DashDirection < loversLeapSpeed)) || (DashDirectionY != 0 && (Player.velocity.Y * DashDirectionY < loversLeapSpeed))) {
						//Player.dashDelay = -1;
						//Player.dash = 2;
						originPlayer.loversLeapDashTime = loversLeapDuration;
						originPlayer.loversLeapDashSpeed = loversLeapSpeed;
						Player.timeSinceLastDashStarted = 0;
						if (DashDirectionY == -1) {
							originPlayer.loversLeapDashDirection = 0;
						} else if (DashDirectionY == 1) {
							originPlayer.loversLeapDashDirection = 1;
						} else if (DashDirection == 1) {
							originPlayer.loversLeapDashDirection = 2;
						} else if (DashDirection == -1) {
							originPlayer.loversLeapDashDirection = 3;
						}
					}
				}
				if (originPlayer.loversLeapDashTime > 0) {
					if (originPlayer.loversLeapDashTime > 1) {
						switch (originPlayer.loversLeapDashDirection) {
							case 0:
							Player.velocity.Y = -originPlayer.loversLeapDashSpeed;
							break;
							case 1:
							Player.velocity.Y = originPlayer.loversLeapDashSpeed;
							break;
							case 2:
							Player.velocity.X = originPlayer.loversLeapDashSpeed;
							break;
							case 3:
							Player.velocity.X = -originPlayer.loversLeapDashSpeed;
							break;
						}
						originPlayer.loversLeapDashTime--;
						originPlayer.dashDelay = 30;
					}
					if ((originPlayer.loversLeapDashTime == 1 || originPlayer.loversLeapDashDirection == 2 || originPlayer.loversLeapDashDirection == 3) && originPlayer.loversLeapDashSpeed > 0) {
						bool bounce = false;
						bool canBounce = true;
						if (Math.Abs(Player.velocity.X) > Math.Abs(Player.velocity.Y)) {
							originPlayer.loversLeapDashDirection = Math.Sign(Player.velocity.X) == 1 ? 2 : 3;
						} else {
							originPlayer.loversLeapDashDirection = Math.Sign(Player.velocity.Y) == 1 ? 1 : 0;
						}
						Rectangle loversLeapHitbox = default;
						int hitDirection = 0;
						switch (originPlayer.loversLeapDashDirection) {
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
											NPC.HitModifiers modifiers = npc.GetIncomingStrikeModifiers(originPlayer.loversLeapItem.DamageType, hitDirection);

											Player.ApplyBannerOffenseBuff(npc, ref modifiers);
											if (npc.life > 5) {
												Player.OnHit(npc.Center.X, npc.Center.Y, npc);
											}
											modifiers.ArmorPenetration += Player.GetWeaponArmorPenetration(originPlayer.loversLeapItem);
											CombinedHooks.ModifyPlayerHitNPCWithItem(Player, originPlayer.loversLeapItem, npc, ref modifiers);

											NPC.HitInfo strike = modifiers.ToHitInfo(Player.GetWeaponDamage(originPlayer.loversLeapItem), Main.rand.Next(100) < Player.GetWeaponCrit(originPlayer.loversLeapItem), Player.GetWeaponKnockback(originPlayer.loversLeapItem), damageVariation: true, Player.luck);
											NPCKillAttempt attempt = new(npc);
											int dmgDealt = npc.StrikeNPC(strike);

											CombinedHooks.OnPlayerHitNPCWithItem(Player, originPlayer.loversLeapItem, npc, in strike, dmgDealt);
											PlayerMethods.ApplyNPCOnHitEffects(Player, originPlayer.loversLeapItem, Item.GetDrawHitbox(originPlayer.loversLeapItem.type, Player), strike.SourceDamage, strike.Knockback, npc.whoAmI, strike.SourceDamage, dmgDealt);
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
												Player.OnKillNPC(ref attempt, originPlayer.loversLeapItem);
											}
											Player.GiveImmuneTimeForCollisionAttack(4);
										}
									}
								}
							}
							if (bounce) {
								originPlayer.loversLeapDashDirection ^= 1;
								originPlayer.loversLeapDashTime = 2;
								originPlayer.loversLeapDashSpeed = Math.Min(originPlayer.loversLeapDashSpeed - 0.5f, 9f);
							}
							switch (originPlayer.loversLeapDashDirection) {
								case 2:
								case 3:
								if (bounce) {
									Player.velocity.Y -= 4;
									originPlayer.loversLeapDashSpeed -= 2f;
								}
								break;
								case 0:
								case 1:
								if (originPlayer.collidingX || originPlayer.collidingY) {
									originPlayer.loversLeapDashTime = 0;
								}
								break;
							}
						}
					} else if (originPlayer.loversLeapDashTime == 1) {
						originPlayer.loversLeapDashTime = 0;
					}
				} else {
					otherDash = true;
				}
			}
			if (originPlayer.dashVase && !otherDash) {
				Player.dashTime = 0;
				const int vaseDashDuration = 12;
				const int vaseDashCooldown = 18;
				float vaseDashSpeed = 8f;
				if (Player.dashDelay <= 0 && DashDirection != 0 && (Player.velocity.X * DashDirection < vaseDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.dashDelay = vaseDashDuration + vaseDashCooldown;
					Player.timeSinceLastDashStarted = 0;
					originPlayer.vaseDashDirection = DashDirection;
					if (Player.velocity.Y * Player.gravDir > Player.gravity * Player.gravDir * 8) {
						Player.velocity.Y = Player.gravity * 8;
					}
				}
				if (Player.dashDelay > vaseDashCooldown && originPlayer.vaseDashDirection != 0) {
					if (Player.velocity.X * originPlayer.vaseDashDirection < vaseDashSpeed) Player.velocity.X = vaseDashSpeed * originPlayer.vaseDashDirection;
					Dust.NewDust(
						Player.position,
						Player.width,
						Player.height,
						DustID.YellowTorch,
						0,
						Player.velocity.Y
					);
				} else {
					originPlayer.vaseDashDirection = 0;
					originPlayer.dashVaseVisual = false;
				}
			} else {
				originPlayer.vaseDashDirection = 0;
				originPlayer.dashVaseVisual = false;
			}
		}
	}
}
