using Origins.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Origins.Items.Tools {
	public class Lightning_Rod : ModItem {

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lightning Rod");
			//Tooltip.SetDefault("Can fish in lava.");
			//ItemID.Sets.CanFishInLava[item.type] = true;
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.ReinforcedFishingPole);
			//Sets the poles fishing power
			item.fishingPole = 37;
			//Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f
			item.shootSpeed = 13.7f;
			item.shoot = ModContent.ProjectileType<Lightning_Rod_Bobber>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
	public class Lightning_Rod_Bobber : ModProjectile {

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lightning Rod Bobber");
		}

		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.BobberReinforced);
			drawOriginOffsetY = -8;
		}

		public override bool PreDrawExtras(SpriteBatch spriteBatch) {
			Color fishingLineColor = new Color(176, 225, 255);
			Lighting.AddLight(projectile.Center, fishingLineColor.R / 255, fishingLineColor.G / 255, fishingLineColor.B / 255);

			//Change these two values in order to change the origin of where the line is being drawn
			int xPositionAdditive = 45;
			float yPositionAdditive = 35f;

			Player player = Main.player[projectile.owner];
			if (!projectile.bobber || player.inventory[player.selectedItem].holdStyle <= 0)
				return false;

			Vector2 lineOrigin = player.MountedCenter;
			lineOrigin.Y += player.gfxOffY;
			int type = player.inventory[player.selectedItem].type;
			//This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (type == ModContent.ItemType<Lightning_Rod>()) {
				lineOrigin.X += xPositionAdditive * player.direction;
				if (player.direction < 0) {
					lineOrigin.X -= 13f;
				}
				lineOrigin.Y -= yPositionAdditive * gravity;
			}

			if (gravity == -1f) {
				lineOrigin.Y -= 12f;
			}
			// RotatedRelativePoint adjusts lineOrigin to account for player rotation.
			lineOrigin = player.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f);
			Vector2 playerToProjectile = projectile.Center - lineOrigin;
			bool canDraw = true;
			if (playerToProjectile.X == 0f && playerToProjectile.Y == 0f)
				return false;

			float playerToProjectileMagnitude = playerToProjectile.Length();
			playerToProjectileMagnitude = 12f / playerToProjectileMagnitude;
			playerToProjectile *= playerToProjectileMagnitude;
			lineOrigin -= playerToProjectile;
			playerToProjectile = projectile.Center - lineOrigin;

			Vector2 lastArcPos = lineOrigin;

			// This math draws the line, while allowing the line to sag.
			int index = 0;
			while (canDraw) {
				index += 1;
				float height = 12f;
				float positionMagnitude = playerToProjectile.Length();
				if (float.IsNaN(positionMagnitude) || float.IsNaN(positionMagnitude))
					break;

				if (positionMagnitude < 20f) {
					height = positionMagnitude - 8f;
					canDraw = false;
				}
				playerToProjectile *= 12f / positionMagnitude;
				lineOrigin += playerToProjectile;
				playerToProjectile.X = projectile.position.X + projectile.width * 0.5f - lineOrigin.X;
				playerToProjectile.Y = projectile.position.Y + projectile.height * 0.1f - lineOrigin.Y;
				if (positionMagnitude > 12f) {
					float positionInverseMultiplier = 0.3f;
					float absVelocitySum = Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y);
					if (absVelocitySum > 16f) {
						absVelocitySum = 16f;
					}
					absVelocitySum = 1f - absVelocitySum / 16f;
					positionInverseMultiplier *= absVelocitySum;
					absVelocitySum = positionMagnitude / 80f;
					if (absVelocitySum > 1f) {
						absVelocitySum = 1f;
					}
					positionInverseMultiplier *= absVelocitySum;
					if (positionInverseMultiplier < 0f) {
						positionInverseMultiplier = 0f;
					}
					absVelocitySum = 1f - projectile.localAI[0] / 100f;
					positionInverseMultiplier *= absVelocitySum;
					if (playerToProjectile.Y > 0f) {
						playerToProjectile.Y *= 1f + positionInverseMultiplier;
						playerToProjectile.X *= 1f - positionInverseMultiplier;
					}
					else {
						absVelocitySum = Math.Abs(projectile.velocity.X) / 3f;
						if (absVelocitySum > 1f) {
							absVelocitySum = 1f;
						}
						absVelocitySum -= 0.5f;
						positionInverseMultiplier *= absVelocitySum;
						if (positionInverseMultiplier > 0f) {
							positionInverseMultiplier *= 2f;
						}
						playerToProjectile.Y *= 1f + positionInverseMultiplier;
						playerToProjectile.X *= 1f - positionInverseMultiplier;
					}
				}
				//This color decides the color of the fishing line. The color is randomized as decided in the AI.
				Color lineColor = Lighting.GetColor((int)lineOrigin.X / 16, (int)(lineOrigin.Y / 16f), fishingLineColor);
				float rotation = playerToProjectile.ToRotation() - MathHelper.PiOver2;
				Main.spriteBatch.Draw(Main.fishingLineTexture, new Vector2(lineOrigin.X - Main.screenPosition.X + Main.fishingLineTexture.Width * 0.5f, lineOrigin.Y - Main.screenPosition.Y + Main.fishingLineTexture.Height * 0.5f), new Rectangle(0, 0, Main.fishingLineTexture.Width, (int)height), lineColor, rotation, new Vector2(Main.fishingLineTexture.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
}