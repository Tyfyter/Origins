using Origins.Items.Pets;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Robot_Toy_Controller : ModItem {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.buffType = buffID;
			Item.shoot = projectileID;
			Item.UseSound = null;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile
			return false;
		}
	}
	public class Toy_Trenchmaker : ModProjectile {
		public bool OnGround {
			get => Projectile.ai[1] != 0;
			set => Projectile.ai[1] = value.ToInt();
		}
		public sbyte CollidingX {
			get => (sbyte)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public bool Flying {
			get => Projectile.ai[2] != 0;
			set => Projectile.ai[2] = value.ToInt();
		}
		public override void SetStaticDefaults() {
			Robot_Toy_Controller.projectileID = Type;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 6;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			ProjectileID.Sets.LightPet[Projectile.type] = false;
			Main.projPet[Type] = true;
		}

		public override void SetDefaults() {
			ProjectileID.Sets.LightPet[Projectile.type] = false;
			Projectile.timeLeft = 5;
			Projectile.width = 36;
			Projectile.height = 38;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.ignoreWater = false;
			DrawOriginOffsetY = 2;
			//Projectile.scale = 1.5f;
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
			if (Projectile.localAI[2] != 1) {
				Projectile.localAI[2] = 1;
				SoundEngine.PlaySound(Origins.Sounds.WindUpToyStart, Projectile.Center, sound => {
					sound.Position = Projectile.Center;
					return true;
				});
				Projectile.soundDelay = 180;
			}

			if (Projectile.soundDelay <= 0 && Main.rand.NextBool(650)) {
				SoundEngine.PlaySound(Origins.Sounds.WindUpToy, Projectile.Center, sound => {
					sound.Position = Projectile.Center;
					return true;
				});
				Projectile.soundDelay = 180;
			}
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Robot_Toy_Controller.buffID);
			}
			if (player.HasBuff(Robot_Toy_Controller.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Center + new Vector2(6 * player.direction, 0);
			idlePosition.X -= 48f * player.direction;
			idlePosition.Y -= 25 * Projectile.scale;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			Vector2 directionToIdlePosition = vectorToIdlePosition.Normalized(out float distanceToIdlePosition);
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 800f) {
				if (distanceToIdlePosition > 2000f) {
					Projectile.position = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
				} else {
					Flying = true;
				}
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Movement
			float speed;
			float inertia;
			if (distanceToIdlePosition > 600f) {
				speed = 16f;
				inertia = 12f;
			} else {
				speed = 6f;
				inertia = 12f;
			}

			if (Flying) {
				Projectile.frameCounter = 0;
				Projectile.frame = 3;
				speed *= 1.5f;
				Min(ref speed, distanceToIdlePosition);
				Vector2 direction = directionToIdlePosition * speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;

				if (Math.Abs(vectorToIdlePosition.X) > 4) Projectile.direction = Math.Sign(vectorToIdlePosition.X);
				else Projectile.direction = player.direction;
				Projectile.spriteDirection = Projectile.direction;
				Projectile.tileCollide = false;
				Vector2 dustDirection = new(Projectile.direction * -3f, 4);
				Vector2 dustPos = Projectile.Center + new Vector2(Projectile.direction * -14, 0);
				for (float i = 0; i < 1; i += 1f / 3) {
					Dust dust = Dust.NewDustPerfect(
						Projectile.Center + new Vector2(Projectile.direction * -14, 0) + Projectile.velocity * (1 - i),
						DustID.Torch
					);
					dust.velocity *= 0.5f;
					dust.velocity += dustDirection;
					dust.position += dust.velocity * i;
					dust.scale -= 0.01f * i;
					dust.noGravity = true;
				}
				if (distanceToIdlePosition > 64 || Projectile.Hitbox.OverlapsAnyTiles()) return;
				if (!Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height + 16)) {
					Rectangle floorbox = Projectile.Hitbox;
					floorbox.Offset(0, Projectile.height);
					floorbox.Height = 16 * 4;
					if (!floorbox.OverlapsAnyTiles(false)) return;
				}
				Flying = false;
				Projectile.netUpdate = true;
			} else {
				Projectile.tileCollide = true;
				int direction = Math.Sign(vectorToIdlePosition.X);
				Projectile.spriteDirection = direction;
				if (vectorToIdlePosition.Y < 160 && vectorToIdlePosition.Y < 48 && CollidingX == direction && OnGround) {
					float jumpStrength = 6;
					if (Collision.TileCollision(Projectile.position - new Vector2(0, 18), new Vector2(4 * direction, 0), Projectile.width, Projectile.height, false, false).X == 0) {
						jumpStrength += 2;
						if (Collision.TileCollision(Projectile.position - new Vector2(0, 36), new Vector2(4 * direction, 0), Projectile.width, Projectile.height, false, false).X == 0) {
							jumpStrength += 2;
						}
					}
					Projectile.velocity.Y = -jumpStrength;
				}
				if (distanceToIdlePosition > 32f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + vectorToIdlePosition.X) / inertia;
				} else {
					inertia /= 2;
					Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1)) / inertia;
				}
			}

			#endregion

			//gravity
			Projectile.velocity.Y += 0.4f;

			#region Animation and visuals
			if (OnGround) {
				Projectile.ai[1]--;
				const int frameDist = 12;
				if (Math.Abs(Projectile.velocity.X) < 0.01f) {
					Projectile.velocity.X = 0f;
				}
				if ((Projectile.velocity.X != 0) ^ (Projectile.oldVelocity.X != 0)) {
					Projectile.frameCounter = 0;
					Projectile.frame = 0;
				}
				if (Projectile.velocity.X != 0) {
					if (Projectile.frameCounter.CycleUp(frameDist, (int)Math.Min(Math.Abs(Projectile.velocity.X), frameDist))) Projectile.frame.CycleUp(Main.projFrames[Type]);
				}
			} else {
				Projectile.frame = 0;
			}
			#endregion
			CollidingX = 0;
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
			return true;
		}
	}
}
namespace Origins.Buffs {
	public class Toy_Trenchmaker_Buff : ModBuff {
		public override string Texture => Origins.TempBuffSprite;
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Robot_Toy_Controller.buffID = Type;
		}
		public override void Update(Player player, ref int buffIndex) { // This method gets called every frame your buff is active on your player.
			player.buffTime[buffIndex] = 18000;

			int projType = Robot_Toy_Controller.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				IEntitySource entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center - new Vector2(48 * player.direction, 0), new Vector2(player.direction, 0), projType, 0, 0f, player.whoAmI);
			}
		}
	}
}
