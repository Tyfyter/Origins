using Microsoft.Xna.Framework;
using Origins.Items.Pets;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Chromatic_Scale : ModItem {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Cyan;// dev items are cyan rarity, despite being expert exclusive
			Item.buffType = buffID;
			Item.shoot = projectileID;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile

			return false;
		}
	}
	public class Chromatic_Pangolin : ModProjectile {
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

		public override void SetStaticDefaults() {
			Chromatic_Scale.projectileID = Type;
			// DisplayName.SetDefault("Chromatic Pangolin");
			//Origins.ExplosiveProjectiles[Projectile.type] = true;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 4;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
		}

		public sealed override void SetDefaults() {
			Projectile.width = 50;
			Projectile.height = 26;
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

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Chromatic_Scale.buffID);
			}
			if (player.HasBuff(Chromatic_Scale.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Bottom;
			idlePosition.X -= 48f * player.direction;
			idlePosition.Y -= 16f * Projectile.scale;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = (idlePosition + new Vector2(6 * player.direction, 0)) - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Projectile.soundDelay == 1) {
				SoundEngine.PlaySound(SoundID.Item29.WithPitch(1f), Projectile.Center);
				Projectile.soundDelay = 0;
			}
			if (distanceToIdlePosition > 500f) {
				if (Main.myPlayer == player.whoAmI) {
					ParticleOrchestrator.RequestParticleSpawn(
						false,
						ParticleOrchestraType.RainbowRodHit,
						new ParticleOrchestraSettings() {
							PositionInWorld = Projectile.Center
						});
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.Center = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
					Projectile.soundDelay = 2;
					ParticleOrchestrator.RequestParticleSpawn(
						false,
						ParticleOrchestraType.RainbowRodHit,
						new ParticleOrchestraSettings() {
							PositionInWorld = Projectile.Center
						});
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

			float speed = 6f;
			float inertia = 12f;
			if (distanceToIdlePosition > 250f) {
				speed = 10f;
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
			if (distanceToIdlePosition > 6f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + vectorToIdlePosition.X) / inertia;
			} else {
				inertia = 6f;
				Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1)) / inertia;
			}
			#endregion

			//gravity
			Projectile.velocity.Y += 0.4f;

			#region Animation and visuals
			if (Projectile.velocity.LengthSquared() < 1) Projectile.spriteDirection = Projectile.direction = player.direction;
			else Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			if (OnGround) {
				Projectile.localAI[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(Projectile.velocity.X) < 0.05f) {
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
				Projectile.frame = 2;
			}
			#endregion
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)Projectile.soundDelay);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.soundDelay = reader.ReadByte();
		}
		public override bool PreKill(int timeLeft) {
			return true;
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
	}
}
namespace Origins.Buffs {
	public class Chromatic_Pangolin_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Chromatic_Scale.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) { // This method gets called every frame your buff is active on your player.
			player.buffTime[buffIndex] = 18000;

			int projType = Chromatic_Scale.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
		static void B<T>(T v) where T : struct, Enum {
			Enum.GetValuesAsUnderlyingType(typeof(T));
		}
	}
}
