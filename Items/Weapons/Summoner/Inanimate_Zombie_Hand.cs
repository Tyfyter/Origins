using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Gores;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
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
	public class Friendly_Zombie_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Friendly_Zombie.ID
		];
		public override bool IsArtifact => true;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Friendly_Zombie : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
		public int MaxLife { get; set; }
		public float Life { get; set; }
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

		public override void SetDefaults() {
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
			Projectile.netImportant = true;
			MaxLife = 200;
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
				player.ClearBuff(Friendly_Zombie_Buff.ID);
			} else if (player.HasBuff(Friendly_Zombie_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			player.tankPet = Projectile.whoAmI;
			player.tankPetReset = false;
			bool walkLeft = Projectile.direction == -1;
			bool walkRight = Projectile.direction == 1;
			bool hasBarrier = false;
			if (player.DistanceSQ(Projectile.Center) > 1000 * 1000) {
				Life--;
			}
			bool foundTarget = false;
			Rectangle targetRect = Projectile.Hitbox;
			Vector2 directionToTarget = Vector2.Zero;
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
				if (foundTarget) {
					targetRect = Main.npc[target].Hitbox;
					targetIsBelow = targetRect.Bottom > Projectile.position.Y + Projectile.height;
				}
				if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
					Projectile.tileCollide = true;
				}
				if (foundTarget && distanceFromTarget < 800 * 800) {
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
					} else if (Projectile.TryJumpOverObstacles(walkRight.ToInt() - walkLeft.ToInt())) {
						Projectile.localAI[1] = Projectile.position.X;
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
			if (Projectile.owner == Main.myPlayer) {
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
					if (hurtAmount > 0) {
						this.DamageArtifactMinion(hurtAmount);
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
		public override void OnKill(int timeLeft) {
			const float spread = 4;
			SoundEngine.PlaySound(SoundID.NPCDeath2, Projectile.Center);
			Player owner = Main.player[Projectile.owner];
			ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(owner.cMinion, owner);
			Dust.NewDustPerfect(
				Projectile.Center + new Vector2(0, -8),
				ModContent.DustType<Friendly_Zombie_Gore1>(),
				Projectile.velocity + Main.rand.NextVector2Circular(spread, spread)
			).shader = shaderData;
			Dust.NewDustPerfect(
				Projectile.Center + new Vector2(0, 0),
				ModContent.DustType<Friendly_Zombie_Gore2>(),
				Projectile.velocity + Main.rand.NextVector2Circular(spread, spread)
			).shader = shaderData;
			Dust.NewDustPerfect(
				Projectile.Center + new Vector2(0, 8),
				ModContent.DustType<Friendly_Zombie_Gore3>(),
				Projectile.velocity + Main.rand.NextVector2Circular(spread, spread)
			).shader = shaderData;
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
		public void OnHurt(int damage, bool fromDoT) {
			if (fromDoT) return;
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
