using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Pets;
using Origins.Tiles;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Wet_Wood : ModItem, ICustomPetFrames {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
			Item.master = true;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(Item.buffType, 3600);
			}
		}
		public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites => [
			("", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Mildew_Blob).GetDefaultTMLName(), AssetRequestMode.ImmediateLoad).Value, Main.projFrames[projectileID], 5)),
		];
	}
	public class Mildew_Blob : ModProjectile {
		public override string Texture => "Origins/Items/Pets/Mildew_Blob";
		public override void SetStaticDefaults() {
			Wet_Wood.projectileID = Projectile.type;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
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
				player.ClearBuff(Wet_Wood.buffID);
			}
			if (player.HasBuff(Wet_Wood.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			float restLeniency = 200f;
			float restYLeniency = 300f;

			Vector2 restPosition = player.Center;
			restPosition.X -= (15 + player.width / 2) * player.direction;
			restPosition.X -= 18 * player.direction;

			if (++Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 0;
			}

			Projectile.rotation += Projectile.velocity.X / 20f;

			Projectile.shouldFallThrough = player.position.Y + player.height - 12f > Projectile.position.Y + Projectile.height;

			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);

			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height)) Projectile.ai[0] = 1f;

			if (Projectile.ai[0] == 1f) {
				Projectile.tileCollide = false;
				float acceleration = 0.2f;
				float targetSpeed = 10f;
				int num19 = 200;
				float playerSpeed = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y);
				if (targetSpeed < playerSpeed) targetSpeed = playerSpeed;

				Vector2 targetVelocity = player.Center - Projectile.Center;
				float distance = targetVelocity.Length();
				if (distance > 2000f)
					Projectile.position = player.Center - Projectile.Size * 0.5f;

				if (distance < num19 && player.velocity.Y == 0f && Projectile.position.Y + Projectile.height <= player.position.Y + player.height && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
					Projectile.ai[0] = 0f;
					Projectile.netUpdate = true;
					if (Projectile.velocity.Y < -6f)
						Projectile.velocity.Y = -6f;
				}

				if (!(distance < 60f)) {
					targetVelocity.Normalize();
					targetVelocity *= targetSpeed;
					if (Projectile.velocity.X < targetVelocity.X) {
						Projectile.velocity.X += acceleration;
						if (Projectile.velocity.X < 0f)
							Projectile.velocity.X += acceleration * 1.5f;
					}

					if (Projectile.velocity.X > targetVelocity.X) {
						Projectile.velocity.X -= acceleration;
						if (Projectile.velocity.X > 0f)
							Projectile.velocity.X -= acceleration * 1.5f;
					}

					if (Projectile.velocity.Y < targetVelocity.Y) {
						Projectile.velocity.Y += acceleration;
						if (Projectile.velocity.Y < 0f)
							Projectile.velocity.Y += acceleration * 1.5f;
					}

					if (Projectile.velocity.Y > targetVelocity.Y) {
						Projectile.velocity.Y -= acceleration;
						if (Projectile.velocity.Y > 0f)
							Projectile.velocity.Y -= acceleration * 1.5f;
					}
				}

				if (Projectile.velocity.X != 0f)
					Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}

			if (Projectile.ai[0] == 0f) {
				if (player.rocketDelay2 > 0) {
					Projectile.ai[0] = 1f;
					Projectile.netUpdate = true;
				}

				Vector2 projOffset = player.Center - Projectile.Center;
				if (projOffset.Length() > 2000f) {
					Projectile.position = player.Center - new Vector2(Projectile.width, Projectile.height) / 2f;
				} else if (!projOffset.IsWithin(Vector2.Zero, restLeniency) || Math.Abs(projOffset.Y) > restYLeniency) {
					Projectile.ai[0] = 1f;
					Projectile.netUpdate = true;
					if (Projectile.velocity.Y > 0f && projOffset.Y < 0f)
						Projectile.velocity.Y = 0f;

					if (Projectile.velocity.Y < 0f && projOffset.Y > 0f)
						Projectile.velocity.Y = 0f;
				}
			}

			if (Projectile.ai[0] == 0f) {
				if (Projectile.Distance(player.Center) > 60f && Projectile.Distance(restPosition) > 60f && Math.Sign(restPosition.X - player.Center.X) != Math.Sign(Projectile.Center.X - player.Center.X))
					restPosition = player.Center;

				{
					Rectangle restArea = Utils.CenteredRectangle(restPosition, Projectile.Size);
					for (int i = 0; i < 20; i++) {
						if (Collision.SolidCollision(restArea.TopLeft(), restArea.Width, restArea.Height))
							break;

						restArea.Y += 16;
						restPosition.Y += 16f;
					}
				}

				Vector2 reachableOffset = Collision.TileCollision(player.Center - Projectile.Size / 2f, restPosition - player.Center, Projectile.width, Projectile.height);
				restPosition = player.Center - Projectile.Size / 2f + reachableOffset;
				if (Projectile.Distance(restPosition) < 32f) {
					if (player.Center.Distance(Projectile.Center) < player.Center.Distance(restPosition))
						restPosition = Projectile.Center;
				}

				Vector2 restOffset = player.Center - restPosition;
				if (!restOffset.IsWithin(Vector2.Zero, restLeniency) || Math.Abs(restOffset.Y) > restYLeniency) {
					Rectangle restArea = Utils.CenteredRectangle(player.Center, Projectile.Size);
					Vector2 vector10 = restPosition - player.Center;
					Vector2 vector11 = restArea.TopLeft();
					for (float i = 0f; i < 1f; i += 0.05f) {
						Vector2 vector12 = restArea.TopLeft() + vector10 * i;
						if (Collision.SolidCollision(restArea.TopLeft() + vector10 * i, restArea.Width, restArea.Height))
							break;

						vector11 = vector12;
					}

					restPosition = vector11 + Projectile.Size / 2f;
				}

				Projectile.tileCollide = true;
				float acceleration = 0.5f;
				float maxSpeed = 4f;
				float deceleration = 0.1f;

				if (maxSpeed < Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y)) {
					maxSpeed = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y);
					acceleration = 0.7f;
				}

				int moveDirection = 0;
				bool shouldJump = Projectile.velocity.X != 0f;
				float xDiff = restPosition.X - Projectile.Center.X;
				Vector2 vectorToRestPosition = restPosition - Projectile.Center;
				if (Math.Abs(xDiff) > 5f) {
					if (xDiff < 0f) {
						moveDirection = -1;
						if (Projectile.velocity.X > -4)
							Projectile.velocity.X -= acceleration;
						else
							Projectile.velocity.X -= deceleration;
					} else {
						moveDirection = 1;
						if (Projectile.velocity.X < 4)
							Projectile.velocity.X += acceleration;
						else
							Projectile.velocity.X += deceleration;
					}
				} else {
					Projectile.velocity.X *= 0.9f;
					if (Math.Abs(Projectile.velocity.X) < acceleration * 2f)
						Projectile.velocity.X = 0f;
				}

				Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
				float num43 = Utils.GetLerpValue(0f, 100f, vectorToRestPosition.Y, clamped: true) * Utils.GetLerpValue(-2f, -6f, Projectile.velocity.Y, clamped: true);
				if (Projectile.velocity.Y == 0f) {
					if (shouldJump) {
						for (int k = 0; k < 3; k++) {
							int checkPos;
							switch (k) {
								case 0:
								checkPos = (int)Projectile.position.X / 16;
								break;

								default:
								checkPos = (int)Projectile.Center.X / 16;
								break;

								case 2:
								checkPos = (int)(Projectile.position.X + Projectile.width) / 16;
								break;
							}

							int checkY = (int)(Projectile.position.Y + (float)Projectile.height) / 16;
							if (!WorldGen.SolidTile(checkPos, checkY) && !Main.tile[checkPos, checkY].IsHalfBlock && Main.tile[checkPos, checkY].Slope <= 0 && (!TileID.Sets.Platforms[Main.tile[checkPos, checkY].TileType] || !Main.tile[checkPos, checkY].HasUnactuatedTile))
								continue;

							try {
								checkPos = (int)Projectile.Center.X / 16;
								checkY = (int)Projectile.Center.Y / 16;
								checkPos += moveDirection;
								checkPos += (int)Projectile.velocity.X;
								if (!WorldGen.SolidTile(checkPos, checkY - 1) && !WorldGen.SolidTile(checkPos, checkY - 2))
									Projectile.velocity.Y = -5.1f;
								else if (!WorldGen.SolidTile(checkPos, checkY - 2))
									Projectile.velocity.Y = -7.1f;
								else if (WorldGen.SolidTile(checkPos, checkY - 5))
									Projectile.velocity.Y = -11.1f;
								else if (WorldGen.SolidTile(checkPos, checkY - 4))
									Projectile.velocity.Y = -10.1f;
								else
									Projectile.velocity.Y = -9.1f;
							} catch {
								Projectile.velocity.Y = -9.1f;
							}
						}

						if (restPosition.Y - Projectile.Center.Y < -48f) {
							float restYDiff = restPosition.Y - Projectile.Center.Y;
							restYDiff *= -1f;
							if (restYDiff < 60f)
								Projectile.velocity.Y = -6f;
							else if (restYDiff < 80f)
								Projectile.velocity.Y = -7f;
							else if (restYDiff < 100f)
								Projectile.velocity.Y = -8f;
							else if (restYDiff < 120f)
								Projectile.velocity.Y = -9f;
							else if (restYDiff < 140f)
								Projectile.velocity.Y = -10f;
							else if (restYDiff < 160f)
								Projectile.velocity.Y = -11f;
							else if (restYDiff < 190f)
								Projectile.velocity.Y = -12f;
							else if (restYDiff < 210f)
								Projectile.velocity.Y = -13f;
							else if (restYDiff < 270f)
								Projectile.velocity.Y = -14f;
							else if (restYDiff < 310f)
								Projectile.velocity.Y = -15f;
							else
								Projectile.velocity.Y = -16f;
						}
					}
				}

				if (Projectile.velocity.X > maxSpeed)
					Projectile.velocity.X = maxSpeed;

				if (Projectile.velocity.X < 0f - maxSpeed)
					Projectile.velocity.X = 0f - maxSpeed;

				if (Projectile.velocity.X < 0f)
					Projectile.direction = -1;

				if (Projectile.velocity.X > 0f)
					Projectile.direction = 1;

				if (Projectile.velocity.X == 0f)
					Projectile.direction = ((player.Center.X > Projectile.Center.X) ? 1 : (-1));

				if (Projectile.velocity.X > acceleration && moveDirection == 1)
					Projectile.direction = 1;

				if (Projectile.velocity.X < 0f - acceleration && moveDirection == -1)
					Projectile.direction = -1;

				Projectile.spriteDirection = Projectile.direction;
				Projectile.velocity.Y += 0.4f + num43;
				if (Projectile.velocity.Y > 10f)
					Projectile.velocity.Y = 10f;
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = Main.player[Projectile.owner].Bottom.Y > Projectile.Bottom.Y + 4f;
			return true;
		}
	}
}

namespace Origins.Buffs {
	public class Mildew_Blob_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Mildew_Blob_Buff";
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Wet_Wood.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;
			Main.vanityPet[Type] = true;
			Main.lightPet[Type] = false;

			int projType = Wet_Wood.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}