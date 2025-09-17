using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Projectiles;
using System.Collections.Generic;
using Origins.Buffs;
using Origins.NPCs.MiscE;

namespace Origins.Items.Weapons.Summoner {
	public class Bomb_Artifact : ModItem, ICustomWikiStat {
		internal static int projectileID = 0;
        public string[] Categories => [
            "Artifact",
			"Minion"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 65;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Item.mana = 36;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Friendly_Bomb_Buff.ID;
			Item.shoot = projectileID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage + (int)player.GetTotalDamage(Item.DamageType).CombineWith(player.OriginPlayer().artifactDamage).GetInverse().ApplyTo(Item.damage), knockback);
			//var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			//projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Friendly_Bomb_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Bomb_Artifact.projectileID
		];
		public override bool IsArtifact => true;
		public override bool DrawHealthBars => false;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Friendly_Bomb : ModProjectile, IArtifactMinion {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Happy_Boi";
		public bool OnGround {
			get {
				return Projectile.localAI[1] > 0;
			}
			set {
				Projectile.localAI[1] = value ? 2 : 0;
			}
		}
		public sbyte CollidingX {
			get {
				return (sbyte)Projectile.localAI[0];
			}
			set {
				Projectile.localAI[0] = value;
			}
		}

		public int MaxLife { get; set; }
		public float Life { get; set; }
		public override void SetStaticDefaults() {
			Bomb_Artifact.projectileID = Type;
			//Origins.ExplosiveProjectiles[Projectile.type] = true;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 11;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			Origins.ForceFelnumShockOnShoot[Type] = true;
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.width = 30;
			Projectile.height = 48;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.ignoreWater = false;
			Projectile.netImportant = true;
			MaxLife = 1;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Friendly_Bomb_Buff.ID);
			}
			if (player.HasBuff(Friendly_Bomb_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Bottom;
			idlePosition.X -= 48f * player.direction + player.direction * Projectile.width * (Projectile.minionPos + 1);
			idlePosition.Y -= 25 * Projectile.scale;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = (idlePosition + new Vector2(6 * player.direction, 0)) - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (distanceToIdlePosition > 1000f) {
				if (++Projectile.ai[1] > 3600f) {
					ModContent.GetInstance<Abandoned_Bomb_System>().BombPositions.Add((Projectile.Center.ToTileCoordinates(), player.OriginPlayer().guid));
					Projectile.active = false;
				}
				if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.position = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
				}
			} else {
				Projectile.ai[1] = 0;
			}

			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type == Type && other.owner == Projectile.owner && other.Hitbox.Intersects(Projectile.Hitbox)) {
					Projectile.velocity.X += Math.Sign(Projectile.position.X - other.position.X) * 0.06f;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700f) {
					distanceFromTarget = 700f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(npc.Center, Projectile.Center);
					bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
					bool inRange = between < distanceFromTarget;
					bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
					// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
					// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
					bool closeThroughWall = between < 100f;
					if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);

			//projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 6f;

			if (foundTarget) {
				// Minion has a target: attack (here, fly towards the enemy)
				Vector2 direction = targetCenter - Projectile.Center;
				int directionX = Math.Sign(direction.X);
				Projectile.spriteDirection = directionX;
				bool wallColliding = CollidingX == directionX;
				float YRatio = direction.Y / ((direction.X * -directionX) + 0.1f);
				if (direction.Y < 160 && (wallColliding || YRatio > 1) && OnGround) {
					float jumpStrength = 6;
					if (wallColliding) {
						if (Collision.TileCollision(Projectile.position - new Vector2(18), Vector2.UnitX, Projectile.width, Projectile.height, false, false).X == 0) {
							jumpStrength++;
							if (Collision.TileCollision(Projectile.position - new Vector2(36), Vector2.UnitX, Projectile.width, Projectile.height, false, false).X == 0) {
								jumpStrength++;
							}
						}
					} else {
						if (YRatio > 1.1f) {
							jumpStrength++;
							if (YRatio > 1.2f) {
								jumpStrength++;
								if (YRatio > 1.3f) {
									jumpStrength++;
								}
							}
						}
					}
					Projectile.velocity.Y = -jumpStrength;
				}
				Rectangle hitbox = new Rectangle((int)Projectile.Center.X - 24, (int)Projectile.Center.Y - 24, 48, 48);
				NPC targetNPC = Main.npc[target];
				int targetMovDir = Math.Sign(targetNPC.velocity.X);
				Rectangle targetFutureHitbox = targetNPC.Hitbox;
				targetFutureHitbox.X += (int)(targetNPC.velocity.X * 10);
				bool waitForStart = (targetMovDir == directionX && !hitbox.Intersects(targetFutureHitbox));
				if (distanceFromTarget > 48f || !hitbox.Intersects(targetNPC.Hitbox) || (waitForStart && Projectile.ai[0] <= 0f)) {
					Projectile.ai[0] = 0f;
					direction.Normalize();
					direction *= speed;
					Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + direction.X) / inertia;
				} else {
					Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1)) / inertia;
					if (Projectile.ai[0] <= 0f) {
						Projectile.ai[0] = 1;
					}
				}
			} else {
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
					inertia = 12f;
				} else {
					speed = 6f;
					inertia = 12f;
				}
				int direction = Math.Sign(vectorToIdlePosition.X);
				Projectile.spriteDirection = direction;
				if (vectorToIdlePosition.Y < 160 && CollidingX == direction && OnGround) {
					float jumpStrength = 6;
					if (Collision.TileCollision(Projectile.position - new Vector2(0, 18), new Vector2(4 * direction, 0), Projectile.width, Projectile.height, false, false).X == 0) {
						jumpStrength += 2;
						if (Collision.TileCollision(Projectile.position - new Vector2(0, 36), new Vector2(4 * direction, 0), Projectile.width, Projectile.height, false, false).X == 0) {
							jumpStrength += 2;
						}
					}
					Projectile.velocity.Y = -jumpStrength;
				}
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + vectorToIdlePosition.X) / inertia;
				} else {
					Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1)) / inertia;
				}
			}
			#endregion

			//gravity
			Projectile.velocity.Y += 0.4f;

			#region Animation and visuals
			if (Projectile.ai[0] > 0) {
				Projectile.frame = 7 + (int)(Projectile.ai[0] / 6f);
				if (++Projectile.ai[0] > 30) {
					//Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, projectile.damage, 0, projectile.owner, 1, 1);
					Projectile.DamageArtifactMinion(50, noCombatText: true);
				}
			} else if (OnGround) {
				Projectile.localAI[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(Projectile.velocity.X) < 0.01f) {
					Projectile.velocity.X = 0f;
				}
				if ((Projectile.velocity.X != 0) ^ (Projectile.oldVelocity.X != 0)) {
					Projectile.frameCounter = 0;
				}
				if (Projectile.velocity.X != 0) {
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						Projectile.frame++;
						if (Projectile.frame >= 6) {
							Projectile.frame = 0;
						}
					}
				} else {
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						Projectile.frame = 0;
					}
				}
			} else if (Projectile.frame > 6) {
				Projectile.frame = 1;
			}

			// Some visuals here
			if (Projectile.frame < 7) {
				Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 0.18f);
			} else if (Projectile.frame < 9) {
				Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.24f);
			}
			#endregion
		}
		public override void PrepareBombToBlow() {
			Projectile.Resize(128, 128);
		}
		public override void OnKill(int timeLeft) {
			Projectile.friendly = true;
			Projectile.PrepareBombToBlow();
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
			Projectile.Damage();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (oldVelocity.Y > Projectile.velocity.Y) {
				OnGround = true;
			} else {
				if (Collision.SlopeCollision(Projectile.position, new Vector2(0, 4), Projectile.width, Projectile.height).Y != 4) {
					OnGround = true;
				}
			}
			if (oldVelocity.X > Projectile.velocity.X) {
				CollidingX = (sbyte)(1 - Collision.TileCollision(Projectile.position, Vector2.UnitX, Projectile.width, Projectile.height, false, false).X);
			} else if (oldVelocity.X < Projectile.velocity.X) {
				CollidingX = (sbyte)(-1 - Collision.TileCollision(Projectile.position, -Vector2.UnitX, Projectile.width, Projectile.height, false, false).X);
			} else {
				CollidingX = 0;
			}
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, Projectile.frame * 52, 56, 50),
				Projectile.GetAlpha(lightColor),
				Projectile.rotation,
				new Vector2(28, 25),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			Main.EntitySpriteDraw(
				Mod.Assets.Request<Texture2D>("Items/Weapons/Summoner/Minions/Happy_Boi_Glow").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, Projectile.frame * 52, 56, 50),
				Color.White,
				Projectile.rotation,
				new Vector2(28, 25),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			return false;
		}
	}
}
