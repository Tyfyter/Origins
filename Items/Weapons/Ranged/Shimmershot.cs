using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Shimmershot : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.gunProj[Type] = true;
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Handgun);
			Item.damage = 54;
			Item.knockBack = 8;
			Item.crit = 15;
			Item.useTime = Item.useAnimation = 36;
			Item.shoot = ModContent.ProjectileType<Shimmershot_P>();
			Item.shootSpeed = 16;
			Item.width = 38;
			Item.height = 18;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = null;
		}
		public bool? Hardmode => false;
		public static bool isShooting = false;
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (source.Context?.Contains("gunProj") != true) {
				Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, ai1: 1, ai2: player.altFunctionUse == 2 ? -1 : 0);
				return false;
			}
			return true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Aetherite_Bar>(13)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Shimmershot_P : ModProjectile {
		public override string Texture => typeof(Shimmershot).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.aiStyle = ProjAIStyleID.HeldProjectile;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.OriginPlayer();
			if (Projectile.ai[2] == 1) {
				SoundEngine.PlaySound(SoundID.Item67.WithPitch(-2f), Projectile.position);
				SoundEngine.PlaySound(SoundID.Item142, Projectile.position);
				SoundEngine.PlaySound(Origins.Sounds.HeavyCannon, Projectile.position);
				Projectile.ai[2] = 0;
			}
			if (!player.noItems && !player.CCed) {
				if (Main.myPlayer == Projectile.owner) {
					Vector2 position = player.MountedCenter + (new Vector2(8, -6 * player.direction).RotatedBy(Projectile.rotation - MathHelper.PiOver2)).Floor();
					Projectile.position = position;
					Vector2 direction = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - position;
					if (player.gravDir == -1f) direction.Y = (Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - position.Y;

					Vector2 velocity = Vector2.Normalize(direction);
					if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
					if (Projectile.velocity != velocity) {
						Projectile.velocity = velocity;
						Projectile.netUpdate = true;
					}
					Projectile.rotation = velocity.ToRotation();
				}
				if (--Projectile.ai[0] <= 0) {
					if (Projectile.ai[2] != -1) {
						if (Main.mouseLeft && Projectile.ai[2] != -1) {
							ActuallyShoot();
						} else if (!player.channel) {
							Projectile.Kill();
						}
					} else {
						Projectile.ai[2] = 0;
					}
				}
				if (Projectile.localAI[0] != Projectile.ai[0]) {
					if (Projectile.ai[0] > Projectile.localAI[0]) Projectile.localAI[1] = Projectile.ai[0];
					Projectile.localAI[0] = Projectile.ai[0];
				}
			} else {
				Projectile.Kill();
			}
			//Projectile.position.Y += player.gravDir * 2f;
		}
		bool ActuallyShoot() {
			Player player = Main.player[Projectile.owner];
			Vector2 position = Projectile.position;
			if (player.PickAmmo(player.HeldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemId)) {
				EntitySource_ItemUse_WithAmmo projectileSource = new(player, player.HeldItem, usedAmmoItemId, "gunProj");
				Vector2 velocity = Projectile.velocity;
				velocity *= speed;

				position += Projectile.velocity * 37 + new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) * 6 * player.direction;
				CombinedHooks.ModifyShootStats(player, player.HeldItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockBack);
				try {
					Shimmershot.isShooting = true;
					if (CombinedHooks.Shoot(player, player.HeldItem, projectileSource, position, velocity, projToShoot, damage, knockBack)) {
						Projectile.NewProjectile(projectileSource, position, velocity, projToShoot, damage, knockBack, Projectile.owner);
					}
				} finally {
					Shimmershot.isShooting = false;
				}
				Projectile.ai[2] = 1;
				Projectile.ai[0] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem);
				Projectile.netUpdate = true;
				return true;
			}
			return false;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects dir = Main.player[Projectile.owner].direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (Main.player[Projectile.owner].gravDir == -1f) {
				dir ^= SpriteEffects.FlipVertically;
			}
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new Vector2(27, 12);
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				Projectile.position - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				origin.Apply(dir, frame.Size()),
				Projectile.scale,
				dir
			);
			return false;
		}
	}
}
