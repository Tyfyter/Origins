using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Journal;
using Origins.Projectiles;
using Origins.Projectiles.Weapons;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Projectiles.ArtifactMinionExtensions;

namespace Origins.Items.Weapons.Summoner {
	public class Old_Brine_Rock : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Old_Brine_Rock_Entry).Name;
		public class Old_Brine_Rock_Entry : JournalEntry {
			public override string TextKey => "Old_Brine_Rock";
			public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 1);
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 40;
			Item.knockBack = 6f;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 38;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Old_Turtle_Buff.ID;
			Item.shoot = Old_Turtle.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Old_Turtle_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Old_Turtle.ID
		];
		public override bool IsArtifact => true;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Old_Turtle : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public bool CanDie => true;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 14;
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Main.projFrames[Type] = 14;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 46;
			Projectile.height = 36;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.manualDirectionChange = true;
			Projectile.netImportant = true;
			Projectile.GetGlobalProjectile<ArtifactMinionGlobalProjectile>().defense += 20;
			MaxLife = 1000;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.velocity.X < 0) {
				Projectile.direction = -1;
			} else {
				Projectile.direction = 1;
			}
		}
		bool targetIsBelow = false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Old_Turtle_Buff.ID);
			} else if (player.HasBuff(Old_Turtle_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			bool foundTarget = false;
			float distanceFromTarget = 1200 * 1200;
			int target = -1;
			Rectangle targetRect = Projectile.Hitbox;
			Vector2 directionToTarget = Vector2.Zero;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700 * 700) {
					distanceFromTarget = 700 * 700;
				}
				if (!npc.CanBeChasedBy(Projectile)) return;
				float between = Vector2.DistanceSquared(npc.Center, Projectile.Center);
				if (between < distanceFromTarget) {
					distanceFromTarget = between;
					target = npc.whoAmI;
					foundTarget = true;
				}
			}
			foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			if (foundTarget) {
				targetRect = Main.npc[target].Hitbox;
				targetIsBelow = targetRect.Bottom > Projectile.position.Y + Projectile.height;
			} else {
				Vector2 idlePosition = player.Top;
				idlePosition.X -= (48f + Projectile.minionPos * 48) * player.direction;
				targetRect = new Rectangle((int)idlePosition.X, (int)idlePosition.Y, 0, 0);
				targetIsBelow = targetRect.Bottom > Projectile.position.Y + Projectile.height;
			}
			if (Projectile.wet) {
				if (Projectile.ai[2] == 0) {
					Projectile.rotation = MathHelper.PiOver2 - MathHelper.PiOver2 * Math.Sign(Projectile.velocity.X);
					Projectile.ai[2] = 1;
				}
				Vector2 direction = Projectile.DirectionTo(targetRect.Center());
				/*if (foundTarget) {
					direction = Projectile.DirectionTo(targetRect.Center());
				} else {
					Projectile.direction = Math.Sign(Projectile.velocity.X);
					if (Projectile.direction == 0) Projectile.direction = 1;
					direction = Vector2.UnitX * Projectile.direction;
				}*/
				const float dist = 16 * 4;
				float tileAvoidance = 0;
				for (int i = -3; i < 4; i++) {
					if (i == 0) continue;
					Vector2 dir = Vector2.UnitX.RotatedBy(Projectile.rotation + i * 0.5f * Projectile.direction);
					float newDist = CollisionExt.Raymarch(Projectile.Center, dir, dist);
					//OriginExtensions.DrawDebugLine(NPC.Center, NPC.Center + dir * newDist);
					if (newDist < dist && Framing.GetTileSafely(Projectile.Center + dir * (newDist + 2)).HasFullSolidTile()) {
						tileAvoidance += dist / (newDist * i + 1);
					}
				}
				if (tileAvoidance != 0) {
					direction = direction.RotatedBy(Math.Clamp(tileAvoidance * -0.5f * Projectile.direction, -MathHelper.PiOver2, MathHelper.PiOver2));
				}
				float oldRot = Projectile.rotation;
				direction = Vector2.Lerp(direction, new Vector2(Math.Sign(direction.X), 0), Math.Abs(direction.Y) - 0.5f).SafeNormalize(direction);
				GeometryUtils.AngularSmoothing(ref Projectile.rotation, direction.ToRotation(), 0.1f);
				float diff = GeometryUtils.AngleDif(oldRot, Projectile.rotation, out int turnDir) * 0.75f;
				if (Math.Abs(diff) < MathHelper.PiOver2) Projectile.velocity = Projectile.velocity.RotatedBy(diff * turnDir) * (1 - diff * 0.1f);
				Projectile.velocity *= 0.94f;
				Projectile.velocity += direction * 0.2f;
				if (!Collision.WetCollision(Projectile.position + Projectile.velocity, 20, 20)) {
					Projectile.velocity += direction;
				}
				Projectile.spriteDirection = Math.Sign(Math.Cos(Projectile.rotation));
				if (Projectile.velocity.X < -0.8 || Projectile.velocity.X > 0.8) {
					//Projectile.frameCounter++;
					Projectile.frameCounter += Math.Abs((int)Projectile.velocity.X);
					if (Projectile.frame < 8) Projectile.frame = 8;
					if (Projectile.frameCounter > 7) {
						Projectile.frame++;
						Projectile.frameCounter = 0;
					}
					if (Projectile.frame >= Main.projFrames[Type]) {
						Projectile.frame = 8;
					}
				} else {
					Projectile.frame = 8;
					Projectile.frameCounter = 0;
				}
			} else {
				if (Projectile.ai[2] == 1) {
					Projectile.rotation = 0;
					Projectile.ai[2] = 0;
				}
				bool walkLeft = false;
				bool walkRight = false;
				bool hasBarrier = false;
				if (player.DistanceSQ(Projectile.Center) > 1000 * 1000) {
					Life--;
				}
				if (Projectile.ai[1] > 0f) {
					Projectile.ai[1] -= 1f;
				} else {
					
					if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
						Projectile.tileCollide = true;
					}
					if (distanceFromTarget < 800 * 800) {
						float xDirectionToTarget = targetRect.Center.X - Projectile.Center.X;
						if (xDirectionToTarget < -10f) {
							walkLeft = true;
							walkRight = false;
						} else if (xDirectionToTarget > 10f) {
							walkRight = true;
							walkLeft = false;
						} else if (!foundTarget && Projectile.direction != player.direction) {
							if (player.direction == 1) {
								walkRight = true;
							} else {
								walkLeft = true;
							}
						}
						if (targetRect.Y < Projectile.Center.Y - 100f && Math.Sign(xDirectionToTarget) != -Math.Sign(Projectile.velocity.X) && Math.Abs(xDirectionToTarget) < 50 && Projectile.velocity.Y == 0) {
							Projectile.velocity.Y = -10f;
						}
					}
				}
				if (Projectile.ai[1] != 0f) {
					walkLeft = false;
					walkRight = false;
				}
				Projectile.rotation = 0f;
				float maxSpeed = 6f;
				float acceleration = 0.175f;
				float friction = 0.9f;
				if (walkLeft || walkRight) {
					const float lookahead = 16;
					Vector2 startPos = Projectile.Left;
					Vector2 endPos = default;
					if (walkLeft) {
						startPos.X += 1;
						endPos.X -= lookahead;
					}
					if (walkRight) {
						startPos.X += Projectile.width - 1;
						endPos.X += lookahead;
					}
					endPos += startPos;
					Vector2 offset = Vector2.UnitY * 16;
					if (!CollisionExt.CanHitRay(startPos, endPos) || !CollisionExt.CanHitRay(startPos + offset, endPos + offset) || !CollisionExt.CanHitRay(startPos - offset, endPos - offset)) {
						hasBarrier = true;
					}
				}
				if (Projectile.velocity.Y != 0) {
					friction = 0.97f;
					acceleration *= 0.3f;
				}
				if (walkLeft) {
					if (Projectile.velocity.X > -3.5) {
						Projectile.velocity.X -= acceleration;
					} else {
						Projectile.velocity.X -= acceleration * 0.25f;
					}
				} else if (walkRight) {
					if (Projectile.velocity.X < 3.5) {
						Projectile.velocity.X += acceleration;
					} else {
						Projectile.velocity.X += acceleration * 0.25f;
					}
				} else if (Projectile.velocity.Y == 0) {
					if (Projectile.velocity.X >= -acceleration && Projectile.velocity.X <= acceleration) {
						Projectile.velocity.X = 0f;
					}
				}
				Projectile.velocity.X *= friction;
				Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
				if (Projectile.velocity.Y == 0f) {
					if (hasBarrier) {
						if (Projectile.localAI[1] == Projectile.position.X) {
							(walkLeft, walkRight) = (walkRight, walkLeft);
						} else {
							int groundTileX = (int)(Projectile.position.X + Projectile.width * (walkRight ? 1 : 0)) / 16;
							int groundTileY = (int)(Projectile.position.Y + Projectile.height + 15) / 16;
							if (Framing.GetTileSafely(groundTileX, groundTileY).HasSolidTile()) {
								try {
									if (walkLeft) {
										groundTileX--;
									}
									if (walkRight) {
										groundTileX++;
									}
									groundTileX += (int)Projectile.velocity.X;
									if (!WorldGen.SolidTile(groundTileX, groundTileY - 1) && !WorldGen.SolidTile(groundTileX, groundTileY - 2)) {
										Projectile.velocity.Y = -5.1f;
									} else if (!WorldGen.SolidTile(groundTileX, groundTileY - 2)) {
										Projectile.velocity.Y = -7.1f;
									} else if (WorldGen.SolidTile(groundTileX, groundTileY - 5)) {
										Projectile.velocity.Y = -11.1f;
									} else if (WorldGen.SolidTile(groundTileX, groundTileY - 4)) {
										Projectile.velocity.Y = -10.1f;
									} else {
										Projectile.velocity.Y = -9.1f;
									}
								} catch {
									Projectile.velocity.Y = -9.1f;
								}
								Projectile.localAI[1] = Projectile.position.X;
							}
						}
					}/* else if (hasHole && foundTarget && targetRect.Bottom <= Projectile.position.Y + Projectile.height) {
					Projectile.velocity.Y = -9.1f;
				}*/
				}
				if (Projectile.velocity.X > maxSpeed) {
					Projectile.velocity.X = maxSpeed;
				}
				if (Projectile.velocity.X < -maxSpeed) {
					Projectile.velocity.X = -maxSpeed;
				}
				if (walkLeft != (Projectile.direction == -1) || walkRight != (Projectile.direction == 1)) {
					Projectile.localAI[1] = 0;
				}
				if (walkLeft) {
					Projectile.direction = -1;
				} else if (walkRight) {
					Projectile.direction = 1;
				}

				Projectile.rotation = 0f;
				if (Projectile.velocity.Y >= 0f && Projectile.velocity.Y <= 0.8) {
					if (Projectile.position.X == Projectile.oldPosition.X) {
						Projectile.frame = 0;
						Projectile.frameCounter = 0;
					} else if (Projectile.velocity.X < -0.8 || Projectile.velocity.X > 0.8) {
						//Projectile.frameCounter++;
						Projectile.frameCounter += Math.Abs((int)Projectile.velocity.X);
						if (Projectile.frameCounter > 5) {
							Projectile.frame++;
							Projectile.frameCounter = 0;
						}
						if (Projectile.frame > 7) {
							Projectile.frame = 0;
						}
					} else {
						Projectile.frame = 0;
						Projectile.frameCounter = 0;
					}
				}
				Projectile.velocity.Y += 0.4f;
				if (Projectile.velocity.Y > 10f) {
					Projectile.velocity.Y = 10f;
				}
				if (Projectile.direction != 0) Projectile.spriteDirection = Projectile.direction;
			}
			if (Projectile.owner == Main.myPlayer) {
				if (foundTarget && ++Projectile.localAI[2] > 60 && Projectile.WithinRange(Projectile.Center.Clamp(targetRect), 16 * 5)) {
					int spikeCount = Main.rand.Next(3, 7);
					for (int i = 0; i < spikeCount; i++) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.Center,
							new Vector2(0, -5).RotatedBy(1 - (i / (float)(spikeCount - 1)) * 2),
							ModContent.ProjectileType<Brine_Droplet>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner
						);
					}
					Projectile.localAI[2] = 0;
				}
				Point topLeft = Projectile.position.ToTileCoordinates();
				Point bottomRight = Projectile.BottomRight.ToTileCoordinates();
				int minX = Utils.Clamp(topLeft.X, 0, Main.maxTilesX - 1);
				int minY = Utils.Clamp(topLeft.Y, 0, Main.maxTilesY - 1);
				int maxX = Utils.Clamp(bottomRight.X, 0, Main.maxTilesX - 1) - minX;
				int maxY = Utils.Clamp(bottomRight.Y, 0, Main.maxTilesY - 1) - minY;
				int hurtAmount = 0;
				for (int i = 0; i <= maxX; i++) {
					for (int j = 0; j <= maxY; j++) {
						Tile tile = Main.tile[i + minX, j + minY];
						if (tile.HasTile && TileID.Sets.TouchDamageDestroyTile[tile.TileType]) {
							if (TileID.Sets.TouchDamageImmediate[tile.TileType] > hurtAmount) hurtAmount = TileID.Sets.TouchDamageImmediate[tile.TileType];
							WorldGen.KillTile(i + minX, j + minY);
							if (Main.netMode == NetmodeID.MultiplayerClient && !tile.HasTile) NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 4, i + minX, j + minY);
						}
					}
				}
				if (Projectile.localAI[0] <= 0) {
					if (this.GetHurtByHostiles(skipNPCs: true)) {
						Projectile.localAI[0] = 20;
					} else if (hurtAmount > 0) {
						this.DamageArtifactMinion(hurtAmount, new TileDamageSource());
						Projectile.localAI[0] = 5;
					}
				} else {
					Projectile.localAI[0]--;
				}
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = targetIsBelow;
			return true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 0.5f;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.NPCDeath22, Projectile.Center);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.localAI[0] > 0 && Projectile.localAI[0] % 10 > 5) {
				lightColor *= 0.3f;
			}
			if (!Projectile.wet) return true;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection != 1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 halfSize = new(texture.Width * 0.5f, (texture.Height / Main.projFrames[Type]) * 0.5f);
			Vector2 position = new(Projectile.position.X - Main.screenPosition.X + (Projectile.width / 2) - texture.Width * Projectile.scale / 2f + halfSize.X * Projectile.scale, Projectile.position.Y - Main.screenPosition.Y + Projectile.height - texture.Height * Projectile.scale / Main.projFrames[Type] + 4f + halfSize.Y * Projectile.scale + Projectile.gfxOffY);
			Vector2 origin = new(halfSize.X, halfSize.Y);
			Main.EntitySpriteDraw(
				texture,
				position - Vector2.UnitY * 4,
				texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.rotation,
				origin,
				Projectile.scale,
				spriteEffects,
			0);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.damage > 0 && Projectile.localAI[0] <= 0) {
				hit.HitDirection *= -1;
				hit.Knockback = 2;
				hit.Crit = false;
				Projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
				float oldLife = Life;
				this.DamageArtifactMinion(target.damage, new NPCDamageSource(target));
				if (Life < oldLife) Projectile.localAI[0] = 20;
			}
		}
		public void OnHurt(int damage, bool fromDoT) {
			if (fromDoT) return;
			Player player = Main.player[Projectile.owner];
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
			if (Life > 0) SoundEngine.PlaySound(SoundID.NPCHit19, Projectile.Center);
			for (int i = 0; i < 5; i++) {
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood
				).shader = shader;
			}
		}
	}
}
