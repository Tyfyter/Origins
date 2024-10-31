using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Inanimate_Zombie_Hand : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 28;
			Item.knockBack = 4f;
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
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Friendly_Zombie_Buff.ID;
			Item.shoot = Friendly_Zombie.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Friendly_Zombie_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[Friendly_Zombie.ID] > 0) {
				player.buffTime[buffIndex] = 18000;
			} else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Friendly_Zombie : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
		public int MaxLife { get; set; }
		public int Life { get; set; }
		public bool CanDie => ++Projectile.ai[2] >= 60 * 5;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
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

		public sealed override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 34;
			Projectile.height = 40;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.manualDirectionChange = true;
			MaxLife = 300;
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
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Friendly_Zombie_Buff.ID);
			} else if (player.HasBuff(Friendly_Zombie_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			bool walkLeft = Projectile.direction == -1;
			bool walkRight = Projectile.direction == 1;
			bool hasBarrier = false;
			bool hasHole = false;
			if (player.DistanceSQ(Projectile.Center) > 1000 * 1000) {
				Life--;
			}
			bool foundTarget = false;
			Rectangle targetRect = Projectile.Hitbox;
			Vector2 directionToTarget = Vector2.Zero;
			Projectile.localAI[0] -= 1f;
			if (Projectile.localAI[0] < 0f) {
				Projectile.localAI[0] = 0f;
			}
			if (Projectile.ai[1] > 0f) {
				Projectile.ai[1] -= 1f;
			} else {
				float distanceFromTarget = 1200 * 1200;
				int target = -1;
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
				if (foundTarget) targetRect = Main.npc[target].Hitbox;
				if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
					Projectile.tileCollide = true;
				}
				bool meander = false;
				if (Projectile.localAI[1] > 0f) {
					meander = true;
					Projectile.localAI[1] -= 1f;
				}
				if (foundTarget && distanceFromTarget < 800 * 800) {
					Projectile.localAI[0] = 60;
					float xDirectionToTarget = targetRect.Center.X - Projectile.Center.X;
					if (xDirectionToTarget < -10f) {
						walkLeft = true;
						walkRight = false;
					} else if (xDirectionToTarget > 10f) {
						walkRight = true;
						walkLeft = false;
					}
					if (targetRect.Y < Projectile.Center.Y - 100f && Math.Sign(xDirectionToTarget) != -Math.Sign(Projectile.velocity.X) && Math.Abs(xDirectionToTarget) < 50 && Projectile.velocity.Y == 0) {
						Projectile.velocity.Y = -10f;
					}
					if (meander) {
						if (Projectile.velocity.X < 0f) {
							walkLeft = true;
						} else if (Projectile.velocity.X > 0f) {
							walkRight = true;
						}
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
			if (walkLeft || walkRight) {
				int walkTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
				int walkTileY = (int)(Projectile.position.Y + Projectile.height / 2) / 16;
				if (walkLeft) {
					walkTileX--;
				}
				if (walkRight) {
					walkTileX++;
				}
				if (((walkRight ? 1 : 0) - (walkLeft ? 1 : 0)) == Math.Sign(Projectile.velocity.X) && !Framing.GetTileSafely(walkTileX, (int)(Projectile.position.Y + Projectile.height + 2) / 16).HasSolidTile()) {
					hasHole = true;
				}
				walkTileX += (int)Projectile.velocity.X;
				if (Framing.GetTileSafely(walkTileX, walkTileY).HasFullSolidTile()) {
					hasBarrier = true;
				}
			}
			if (Projectile.velocity.Y == 0) {
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
				} else {
					if (Projectile.velocity.X >= -acceleration && Projectile.velocity.X <= acceleration) {
						Projectile.velocity.X = 0f;
					}
				}
				Projectile.velocity.X *= 0.9f;
			}
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
			if (Projectile.velocity.Y == 0f) {
				if (Projectile.velocity.X < 0f || Projectile.velocity.X > 0f) {
					int num75 = (int)(Projectile.position.X + Projectile.width / 2) / 16;
					int j3 = (int)(Projectile.position.Y + Projectile.height / 2) / 16 + 1;
					if (walkLeft) {
						num75--;
					}
					if (walkRight) {
						num75++;
					}
				}
				if (hasBarrier) {
					int groundTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
					int groundTileY = (int)(Projectile.position.Y + Projectile.height) / 16;
					if (WorldGen.SolidTileAllowBottomSlope(groundTileX, groundTileY) || Main.tile[groundTileX, groundTileY].IsHalfBlock || Main.tile[groundTileX, groundTileY].Slope > 0) {
						try {
							groundTileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
							groundTileY = (int)(Projectile.position.Y + Projectile.height / 2) / 16;
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
					if (Projectile.frame > 2) {
						Projectile.frame = 0;
					}
				} else {
					Projectile.frame = 0;
					Projectile.frameCounter = 0;
				}
			} else {
				Projectile.frameCounter = 0;
				Projectile.frame = 2;
			}
			Projectile.velocity.Y += 0.4f;
			if (Projectile.velocity.Y > 10f) {
				Projectile.velocity.Y = 10f;
			}
			if (Projectile.direction != 0) Projectile.spriteDirection = Projectile.direction;
		}
		public override void OnKill(int timeLeft) {
			const float diameter = 16;
			Player owner = Main.player[Projectile.owner];
			ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(owner.cMinion, owner);
			SoundEngine.PlaySound(SoundID.NPCDeath2, Projectile.Center);
			for (int i = 0; i < 8; i++) {
				Vector2 offset = Main.rand.NextVector2CircularEdge(diameter, diameter) * Main.rand.NextFloat(0.2f, 1f);
				Dust.NewDustPerfect(
					Projectile.Center + offset,
					DustID.TerraBlade,
					offset * 0.125f
				).shader = shaderData;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.damage > 0) {
				hit.HitDirection *= -1;
				hit.Knockback = 2;
				hit.Crit = false;
				Projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
				this.DamageArtifactMinion(target.damage);
			}
		}
		public void OnHurt(int damage) {
			Player player = Main.player[Projectile.owner];
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
			if (Life > 0) SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
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
