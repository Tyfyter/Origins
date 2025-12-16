using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Chambersite_Minigun : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.gunProj[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Handgun);
			Item.damage = 45;
			Item.knockBack = 6;
			Item.useTime = Item.useAnimation = 35;
			Item.shoot = ModContent.ProjectileType<Chambersite_Minigun_P>();
			Item.shootSpeed = 8;
			Item.width = 38;
			Item.height = 18;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item36.WithPitch(1f);
		}
		public static bool isShooting = false;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (source.Context?.Contains("gunProj") != true) {
				Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, ai1: 1);
				return false;
			}
			return true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Ectoplasm, 10)
			.AddIngredient(ItemID.Megashark)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 30)
			.AddIngredient(ModContent.ItemType<Chambersite_Item>(), 13)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Chambersite_Minigun_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
		}
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
			if (Projectile.ai[2] == 2) {
				SoundEngine.PlaySound(player.HeldItem.UseSound, Projectile.position);
				Projectile.ai[2] = 1;
			}
			if (!player.noItems && !player.CCed) {
				if (Main.myPlayer == Projectile.owner) {
					Vector2 position = player.MountedCenter + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 12).Floor();
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
				if (Projectile.ai[1] > 0 && --Projectile.ai[0] <= 0) {
					if (Main.myPlayer == Projectile.owner) {
						if (!player.channel || !ActuallyShoot()) Projectile.Kill();
					}
				}
				if (Projectile.localAI[0] != Projectile.ai[0]) {
					if (Projectile.ai[0] > Projectile.localAI[0]) Projectile.localAI[1] = Projectile.ai[0];
					Projectile.localAI[0] = Projectile.ai[0];
				}
				Projectile.frame = (int)((Projectile.ai[0] / (Projectile.localAI[1] + 1)) * Main.projFrames[Type]);
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
				float spread = 0.7f - Math.Min(Projectile.ai[1], 6) * 0.1f;
				int projectiles = 1;
				if (Projectile.ai[2] == 0) {
					spread = 0.5f;
					projectiles = 5;
				}
				try {
					Chambersite_Minigun.isShooting = true;
					for (; projectiles > 0; projectiles--) {
						Vector2 vel = velocity.RotatedByRandom(spread);
						if (CombinedHooks.Shoot(player, player.HeldItem, projectileSource, position, vel, projToShoot, damage, knockBack)) {
							Projectile.NewProjectile(projectileSource, position, vel, projToShoot, damage, knockBack, Projectile.owner);
						}
					}
				} finally {
					Chambersite_Minigun.isShooting = false;
				}
				Projectile.ai[2] = 2;
				Projectile.ai[1]++;
				Projectile.ai[0] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem) / Math.Min(Projectile.ai[1], 6);
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
			Vector2 origin = new(27, 12);
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
