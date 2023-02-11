using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace Origins.Items.Weapons.Magic {
	public class Pike_of_Deepneus : ModItem {
		public const int baseDamage = 34;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pike of Deepneus");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 75;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Pike_of_Deepneus_P>();
			Item.knockBack = 7;
			Item.shootSpeed = 8;
			Item.mana = 8;
			Item.useStyle = -1;
			Item.useTime = 13;
			Item.useAnimation = 13;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.reuseDelay = 7;
		}
		public override void UseItemFrame(Player player) {
			float rotation = player.itemRotation - MathHelper.PiOver2 - Math.Max((player.itemAnimation / (float)player.itemAnimationMax) * 3 - 2, 0) * (MathHelper.PiOver2 * 0.85f) * player.direction;
			player.SetCompositeArmFront(
				true,
				Player.CompositeArmStretchAmount.Full,
				rotation
			);
			float num23 = rotation * (float)player.direction;
			player.bodyFrame.Y = player.bodyFrame.Height * 3;
			if (num23 < -0.75) {
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				if (player.gravDir == -1f) {
					player.bodyFrame.Y = player.bodyFrame.Height * 4;
				}
			}
			if (num23 > 0.6) {
				player.bodyFrame.Y = player.bodyFrame.Height * 4;
				if (player.gravDir == -1f) {
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				player.whoAmI,
				ai0: player.itemAnimationMax
			);
			return false;
		}
	}
	public class Pike_of_Deepneus_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Pike_of_Deepneus";
		public new AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pike of Deepneus");
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
		}
		public override void Unload() {
			GlowTexture = default;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
			Projectile.width = Projectile.height = 32;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			if (Projectile.ai[0] != 0) {
				Player player = Main.player[Projectile.owner];
				if (Projectile.owner == Main.myPlayer) {
					if (player.channel) {
						Vector2 newVel = (Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter)).SafeNormalize(Vector2.UnitX * player.direction);
						if (player.HeldItem.shoot == Type) {
							newVel *= player.HeldItem.shootSpeed;
						}
						if (newVel != Projectile.velocity) {
							Projectile.netUpdate = true;
						}
						Projectile.velocity = newVel;
						if (Projectile.ai[1] < 1) {
							Projectile.ai[1] += 1 / Projectile.ai[0];
						}
						player.reuseDelay -= (int)(player.reuseDelay * Projectile.ai[1]);
					} else {
						Projectile.ai[0] = 0;
						Projectile.velocity *= 1 + Projectile.ai[1] * 0.5f;
						Projectile.netUpdate = true;
					}
				}
				player.itemRotation = Projectile.velocity.ToRotation();
				player.itemAnimation = (int)(player.itemAnimationMax * (1 + Projectile.ai[1] * 0.25f));
				player.itemTime = player.itemTimeMax = player.itemAnimation;
				player.heldProj = Projectile.whoAmI;
				player.ChangeDir(Projectile.direction = Math.Sign(Projectile.velocity.X));
				Projectile.rotation = player.itemRotation;
				Projectile.Center = player.MountedCenter
					+ OriginExtensions.Vec2FromPolar(player.itemRotation - Math.Max((player.itemAnimation / (float)player.itemAnimationMax) * 3 - 2, 0) * (MathHelper.PiOver2 * 0.85f) * player.direction, 16)
					+ Projectile.velocity.SafeNormalize(default) * 48;
			} else {
				Projectile.hide = false;
				Projectile.tileCollide = true;
				Projectile.velocity.Y += 0.04f;
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.ai[0] != 0) return false;
			return null;
		}
		public override void ModifyDamageScaling(ref float damageScale) {
			damageScale *= 1 + Projectile.ai[1];
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			knockback *= 1 + Projectile.ai[1] * 0.65f;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 14;
			height = 14;
			fallThrough = true;
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 position = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.rotation + (MathHelper.Pi * 0.8f * Projectile.direction - MathHelper.PiOver2);
			Vector2 origin = new Vector2(30 + 25 * Projectile.direction, 9);
			float scale = Projectile.scale;
			SpriteEffects spriteEffects = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				position,
				null,
				lightColor,
				rotation,
				origin,
				scale,
				spriteEffects,
			0);
			if (GlowTexture.IsLoaded) {
				Main.EntitySpriteDraw(
					GlowTexture,
					Projectile.Center - Main.screenPosition,
					null,
					Color.White,
					Projectile.rotation + (0 * Projectile.direction),
					new Vector2(30 + 25 * Projectile.direction, 9),
					Projectile.scale,
					Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
					0
				);
			}
			return false;
		}
	}
}
