using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Lightning_Rod : ModItem {
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ReinforcedFishingPole);
			Item.damage = 0;
			Item.DamageType = DamageClass.Generic;
			Item.noMelee = true;
			//Sets the poles fishing power
			Item.fishingPole = 37;
			//Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f
			Item.shootSpeed = 13.7f;
			Item.shoot = ModContent.ProjectileType<Lightning_Rod_Bobber>();
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Green;
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			/*const float factor = 1.5f;
			damage = new StatModifier(
				(damage.Additive - 1) * factor + 1,
				(damage.Multiplicative - 1) * factor + 1,
				0,
				(damage.Flat + damage.Base) * factor
			);*/
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Lightning_Rod_Bobber : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BobberReinforced);
			DrawOriginOffsetY = -8;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			if (Projectile.ai[0] == 1) {
				Projectile.usesLocalNPCImmunity = false;
				Rectangle biggerHitbox = Projectile.Hitbox;
				biggerHitbox.Inflate(14, 14);
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active && biggerHitbox.Intersects(item.Hitbox)) {
						item.velocity = Projectile.velocity;
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[0] == 1) {
				target.velocity = Vector2.Lerp(target.velocity, Projectile.velocity, target.knockBackResist);
			}
		}
		public override bool PreDrawExtras() {
			Color fishingLineColor = new(176, 225, 255);
			Lighting.AddLight(Projectile.Center, 0, fishingLineColor.G * 0.0015f, fishingLineColor.B * 0.005f);

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
			Vector2 playerToProjectile = Projectile.Center - lineOrigin;
			bool canDraw = true;
			if (playerToProjectile.X == 0f && playerToProjectile.Y == 0f)
				return false;

			float playerToProjectileMagnitude = playerToProjectile.Length();
			playerToProjectileMagnitude = 12f / playerToProjectileMagnitude;
			playerToProjectile *= playerToProjectileMagnitude;
			lineOrigin -= playerToProjectile;
			playerToProjectile = Projectile.Center - lineOrigin;

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
				Color lineColor = Lighting.GetColor((int)lineOrigin.X / 16, (int)(lineOrigin.Y / 16f), fishingLineColor);
				float rotation = playerToProjectile.ToRotation() - MathHelper.PiOver2;
				Texture2D fishingLineTexture = TextureAssets.FishingLine.Value;
				Main.EntitySpriteDraw(fishingLineTexture, new Vector2(lineOrigin.X - Main.screenPosition.X + fishingLineTexture.Width * 0.5f, lineOrigin.Y - Main.screenPosition.Y + fishingLineTexture.Height * 0.5f), new Rectangle(0, 0, fishingLineTexture.Width, (int)height), lineColor, rotation, new Vector2(fishingLineTexture.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f);
				Lighting.AddLight(new Vector2(lineOrigin.X + fishingLineTexture.Width * 0.5f, lineOrigin.Y + fishingLineTexture.Height * 0.5f), 0, fishingLineColor.G * 0.0003f, fishingLineColor.B * 0.001f);
			}
			return false;
		}
	}
}