using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Origins.Buffs;
using PegasusLib;
using Origins.NPCs.Felnum;

namespace Origins.Items.Tools {
	public class Acrid_Laser_Rod : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ReinforcedFishingPole);
			//Sets the poles fishing power
			Item.fishingPole = 43;
			//Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f
			Item.shootSpeed = 15.7f;
			Item.shoot = ModContent.ProjectileType<Acrid_Laser_Rod_Bobber>();
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			player.itemLocation -= new Vector2(player.direction * 10, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			int xPositionAdditive = 58;
			float yPositionAdditive = 31f;
			Vector2 lineOrigin = player.MountedCenter;
			lineOrigin.Y += player.gfxOffY;
			//This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (gravity == -1f) {
				lineOrigin.Y -= 12f;
			}
			lineOrigin.X += xPositionAdditive * player.direction;
			if (player.direction < 0) {
				lineOrigin.X -= 13f;
			}
			lineOrigin.Y -= yPositionAdditive * gravity;
			position = player.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f);
			velocity = (Main.MouseWorld - position).SafeNormalize(default) * velocity.Length();
		}
		public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor) {
			int xPositionAdditive = 58;
			float yPositionAdditive = 31f;
			lineOriginOffset.X += xPositionAdditive;
			lineOriginOffset.Y -= yPositionAdditive;
		}
	}
	public class Acrid_Laser_Rod_Bobber : ModProjectile {
		public AutoLoadingAsset<Texture2D> glowTexture = typeof(Acrid_Laser_Rod_Bobber).GetDefaultTMLName() + "_Glow";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BobberReinforced);
			Projectile.DamageType = DamageClass.Generic;
			DrawOriginOffsetY = -8;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			Color fishingLineColor = new(80, 225, 120);
			Lighting.AddLight(Projectile.Center, fishingLineColor.ToVector3());
			Projectile.extraUpdates = Projectile.ai[1] < 0 ? 0 : 1;
		}
		public override bool PreDrawExtras() {
			Color fishingLineColor = new(80, 225, 120);

			//Change these two values in order to change the origin of where the line is being drawn
			int xPositionAdditive = 45;
			float yPositionAdditive = 33f;

			Player player = Main.player[Projectile.owner];
			if (!Projectile.bobber || player.inventory[player.selectedItem].holdStyle <= 0)
				return false;

			Vector2 lineOrigin = player.MountedCenter;
			lineOrigin.Y += player.gfxOffY;
			int type = player.inventory[player.selectedItem].type;
			//This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (type == ModContent.ItemType<Acrid_Laser_Rod>()) {
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
			Vector2 playerToProjectile = Projectile.Center - lineOrigin;
			bool canDraw = true;
			if (playerToProjectile.X == 0f && playerToProjectile.Y == 0f)
				return false;

			float playerToProjectileMagnitude = playerToProjectile.Length();
			playerToProjectileMagnitude = 12f / playerToProjectileMagnitude;
			playerToProjectile *= playerToProjectileMagnitude;
			lineOrigin -= playerToProjectile;
			playerToProjectile = Projectile.Center - lineOrigin;

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
				playerToProjectile.X = Projectile.position.X + Projectile.width * 0.5f - lineOrigin.X;
				playerToProjectile.Y = Projectile.position.Y + Projectile.height * 0.1f - lineOrigin.Y;
				if (positionMagnitude > 12f) {
					float positionInverseMultiplier = 0.3f;
					float absVelocitySum = Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y);
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
					absVelocitySum = 1f - Projectile.localAI[0] / 100f;
					positionInverseMultiplier *= absVelocitySum;
					if (playerToProjectile.Y > 0f) {
						playerToProjectile.Y *= 1f + positionInverseMultiplier;
						playerToProjectile.X *= 1f - positionInverseMultiplier;
					} else {
						absVelocitySum = Math.Abs(Projectile.velocity.X) / 3f;
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
				float rotation = playerToProjectile.ToRotation() - MathHelper.PiOver2;
				Texture2D fishingLineTexture = TextureAssets.FishingLine.Value;
				Main.EntitySpriteDraw(fishingLineTexture, new Vector2(lineOrigin.X - Main.screenPosition.X + fishingLineTexture.Width * 0.5f, lineOrigin.Y - Main.screenPosition.Y + fishingLineTexture.Height * 0.5f), new Rectangle(0, 0, fishingLineTexture.Width, (int)height), fishingLineColor, rotation, new Vector2(fishingLineTexture.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f);
				Lighting.AddLight(lineOrigin, fishingLineColor.ToVector3() * 0.5f);
			}
			return false;
		}
		public override void PostDraw(Color lightColor) {
			if (!glowTexture.Exists) return;
			float halfAvgWidth = (glowTexture.Value.Width - Projectile.width) * 0.5f + Projectile.width * 0.5f;
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.position + new Vector2(halfAvgWidth + DrawOffsetX, (Projectile.height / 2) + Projectile.gfxOffY) - Main.screenPosition,
				null,
				new Color(250, 250, 250, Projectile.alpha),
				Projectile.rotation,
				new Vector2(halfAvgWidth, Projectile.height / 2  - DrawOriginOffsetY),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
		}
	}
}