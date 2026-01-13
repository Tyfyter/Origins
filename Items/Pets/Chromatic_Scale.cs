using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Pets;
using Origins.Tiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Chromatic_Scale : ModItem, ICustomWikiStat, ICustomPetFrames {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		public bool? Hardmode => false;
		public string[] Categories => [
			"Pet",
			"DeveloperItem"
		];
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.width = 32;
			Item.height = 32;
			Item.rare = AltCyanRarity.ID;// dev items are cyan rarity, despite being expert exclusive
			Item.buffType = buffID;
			Item.shoot = projectileID;
			Item.value = Item.sellPrice(gold: 3, silver: 50);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile
			return false;
		}
		public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites => [
			("", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Chromatic_Pangolin).GetDefaultTMLName(), AssetRequestMode.ImmediateLoad).Value, Main.projFrames[projectileID], 4)),
			("_Dashing", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Chromatic_Pangolin).GetDefaultTMLName() + "_Flying", AssetRequestMode.ImmediateLoad).Value, 3, 4)),
		];
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
		static AutoLoadingAsset<Texture2D> flyingTexture = typeof(Chromatic_Pangolin).GetDefaultTMLName() + "_Flying";
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

		public override void SetDefaults() {
			Projectile.width = 46;
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
				if (distanceToIdlePosition > 2000f) {
					if (Main.myPlayer == player.whoAmI) {
						ParticleOrchestrator.RequestParticleSpawn(
							false,
							ParticleOrchestraType.RainbowRodHit,
							new ParticleOrchestraSettings() { PositionInWorld = Projectile.Center }
						);
						// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
						// and then set netUpdate to true
						Projectile.Center = idlePosition;
						Projectile.velocity *= 0.1f;
						Projectile.netUpdate = true;
						Projectile.soundDelay = 2;
						ParticleOrchestrator.RequestParticleSpawn(
							false,
							ParticleOrchestraType.RainbowRodHit,
							new ParticleOrchestraSettings() { PositionInWorld = Projectile.Center }
						);
					}
				} else {
					Projectile.ai[2] = 1;
					Projectile.netUpdate = true;
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

			float speed = 6f + 6 * Projectile.ai[2];
			float inertia = 12f - 6 * Projectile.ai[2];
			if (distanceToIdlePosition > 250f) {
				speed = 10f + 10f * Projectile.ai[2];
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
			vectorToIdlePosition.Normalize();
			if (distanceToIdlePosition > 6f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				Vector2 dir = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition * speed) / inertia;
				Projectile.velocity.X = dir.X;
				if (Projectile.ai[2] == 1) {
					Projectile.velocity.Y = dir.Y;
				}
			} else {
				inertia = 6f - 3 * Projectile.ai[2];
				Vector2 dir = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition * speed) / inertia;
				Projectile.velocity.X = dir.X;
				if (Projectile.ai[2] == 1) {
					Projectile.velocity.Y = dir.Y;
					Projectile.ai[2] = 0;
				}
			}

			Projectile.tileCollide = Projectile.ai[2] != 1;
			#endregion

			//gravity
			Projectile.velocity.Y += 0.4f;

			#region Animation and visuals
			if (Projectile.velocity.LengthSquared() < 1) Projectile.spriteDirection = Projectile.direction = player.direction;
			else Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);

			if (Projectile.ai[2] != 1) {
				Projectile.velocity.Y += 0.4f;
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.35f);
			} else {
				Projectile.rotation += 1f * Projectile.direction;
			}

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
			} else if (Projectile.ai[2] != 1) {
				Projectile.frame = 2;
			} else {
				if (++Projectile.frameCounter >= 3) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= 3) Projectile.frame = 0;
				}
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
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[2] == 1) {
				Rectangle frame = flyingTexture.Frame(verticalFrames: 3, frameY: Projectile.frame);
				Main.EntitySpriteDraw(
					flyingTexture,
					Projectile.Center - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation,
					frame.Size() * 0.5f,
					1,
					Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
				);
				return false;
			}
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
