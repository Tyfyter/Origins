using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Pets;
using Origins.Tiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Fleshy_Globe : ModItem, ICustomPetFrames {
		internal static int projectileID = 0;
		internal static int buffID = 0;

		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
			Item.buffType = buffID;
			Item.shoot = projectileID;
			Item.UseSound = SoundID.NPCDeath1;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile
			return false;
		}
		public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites => [
			("", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Self_Contained_Universe).GetDefaultTMLName(), AssetRequestMode.ImmediateLoad).Value, Main.projFrames[projectileID], 4)),
		];
	}
	public class Self_Contained_Universe : ModProjectile {
		public bool OnGround {
			get {
				return Projectile.ai[1] > 0;
			}
			set {
				Projectile.ai[1] = value ? 2 : 0;
			}
		}
		public sbyte CollidingX {
			get {
				return (sbyte)Projectile.ai[0];
			}
			set {
				Projectile.ai[0] = value;
			}
		}
		public override void SetStaticDefaults() {
			Fleshy_Globe.projectileID = Type;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 4;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			ProjectileID.Sets.LightPet[Projectile.type] = true;
			Main.projPet[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.timeLeft = 5;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.ignoreWater = false;
			DrawOriginOffsetY = -10;
			DrawOffsetX = -8;
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
			const float lightMult = 1f;
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Fleshy_Globe.buffID);
			}
			if (player.HasBuff(Fleshy_Globe.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Center + new Vector2(6 * player.direction, 0);
			idlePosition.X -= 48f * player.direction;
			idlePosition.Y -= 25 * Projectile.scale;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI) {
				if (distanceToIdlePosition > 1200f) {
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.position = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
				} else if (distanceToIdlePosition > 600) {
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.ai[2] = 1;
					Projectile.netUpdate = true;
				}
			}

			if (Projectile.wet) Projectile.ai[2] = 1;

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

			// Default movement parameters (here for attacking)
			float speed;
			float inertia;
			if (distanceToIdlePosition > 600f) {
				speed = 16f;
				inertia = 12f;
			} else {
				speed = 6f;
				inertia = 12f;
			}
			if (Projectile.ai[2] != 1) {
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
			} else {
				speed *= 1.5f;
				Projectile.tileCollide = false;
			}
			if (distanceToIdlePosition > 32f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Vector2 dir = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				Projectile.velocity.X = dir.X;
				if (Projectile.ai[2] == 1) Projectile.velocity.Y = dir.Y;
			} else {
				inertia /= 2;
				Vector2 dir = (Projectile.velocity * (inertia - 1)) / inertia;
				Projectile.velocity.X = dir.X;
				if (Projectile.ai[2] == 1) {
					Projectile.velocity.Y = dir.Y;
					if (!Projectile.wet) Projectile.ai[2] = 0;
				}
			}
			#endregion

			//gravity
			if (Projectile.ai[2] != 1) {
				Projectile.velocity.Y += 0.4f;
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.35f);
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}

			Vector2 glowPos = Projectile.Center - new Vector2(0, 8);
			Lighting.AddLight(glowPos, Color.Cyan.ToVector3() * lightMult);

			#region Animation and visuals
			if (OnGround) {
				Projectile.ai[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(Projectile.velocity.X) < 0.01f) {
					Projectile.velocity.X = 0f;
				}
				if ((Projectile.velocity.X != 0) ^ (Projectile.oldVelocity.X != 0)) {
					Projectile.frameCounter = 0;
					Projectile.frame = 0;
				}
				if (Projectile.velocity.X != 0) {
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						Projectile.frame++;
						if (Projectile.frame >= 4) {
							Projectile.frame = 0;
						}
					}
				}
			} else {
				if (Projectile.ai[2] == 1) {
					if (Projectile.velocity != Vector2.Zero) {
						int frameSpeed = (int)(24 / Projectile.velocity.Length());
						if (frameSpeed > 0 && ++Projectile.frameCounter >= frameSpeed) {
							Projectile.frameCounter = 0;
							if (++Projectile.frame >= 4) {
								Projectile.frame = 0;
							}
						}
					}
				} else {
					Projectile.frame = 0;
				}
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
	public class Self_Contained_Universe_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.lightPet[Type] = true;
			Fleshy_Globe.buffID = Type;
		}
		public override void Update(Player player, ref int buffIndex) { // This method gets called every frame your buff is active on your player.
			player.buffTime[buffIndex] = 18000;

			int projType = Fleshy_Globe.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center - new Vector2(48 * player.direction, 0), new Vector2(player.direction, 0), projType, 0, 0f, player.whoAmI);
			}
		}
	}
}
