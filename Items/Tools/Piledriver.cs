using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Piledriver : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumDrill);
			Item.damage = 24;
			Item.pick = 70;
			Item.width = 28;
			Item.height = 26;
			Item.useTime = Item.useAnimation = 44;
			Item.knockBack = 9f;
			Item.shootSpeed = 4f;
			Item.shoot = ModContent.ProjectileType<Piledriver_P>();
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 12)
			.AddIngredient(ModContent.ItemType<NE8>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
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
				player.itemTimeMax * player.pickSpeed
			);
			return false;
		}
	}
	public class Piledriver_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.TitaniumDrill);
			Projectile.friendly = false;
		}
		public override void AI() {
			Projectile.rotation += MathHelper.PiOver2;
			if (Projectile.ai[1] >= Projectile.ai[0]) {
				Projectile.ai[1] -= Projectile.ai[0];
				Projectile.frame = 3;
				if (Projectile.owner == Main.myPlayer) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Projectile.velocity,
						ModContent.ProjectileType<Piledriver_Mine_P>(),
						Projectile.originalDamage,
						Projectile.knockBack,
						Main.myPlayer
					);
				}
			} else {
				Projectile.ai[1]++;
				Projectile.frame = (int)((Projectile.ai[1] / Projectile.ai[0]) * 3);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: 4, frameY: Projectile.frame),
				lightColor,
				Projectile.rotation + MathHelper.Pi,
				new Vector2(8, 14),
				Projectile.scale,
				Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically
			);
			return false;
		}
	}
	public class Piledriver_Mine_P : ModProjectile {
		public override string Texture => "Origins/Items/Tools/Piledriver_P";
		public override void SetDefaults() {
			Projectile.timeLeft = 24;
			Projectile.extraUpdates = 100;
			Projectile.width = Projectile.height = 0;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(4, 4);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			oldVelocity /= MathF.Max(MathF.Abs(oldVelocity.X) / 16, MathF.Abs(oldVelocity.Y) / 16);
			Vector2 pos = Projectile.Center;
			Point tilePos = pos.ToTileCoordinates();
			if (!Framing.GetTileSafely(tilePos).HasSolidTile()) pos += oldVelocity;
			Player owner = Main.player[Projectile.owner];
			for (int i = 0; i < 4; i++) {
				tilePos = pos.ToTileCoordinates();
				if (!Framing.GetTileSafely(tilePos).HasSolidTile()) break;
				owner.PickTile(tilePos.X, tilePos.Y, owner.HeldItem.pick);
				pos += oldVelocity;
			}
			return true;
		}
		public override void Kill(int timeLeft) {
			if (timeLeft > 0) {
				Vector2 pos = Projectile.Center;
				for (int i = 0; i < 3; i++) Dust.NewDust(pos, 0, 0, DustID.Torch);
			}
		}
	}
}
