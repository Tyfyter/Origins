using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Beginners_Tome : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Beginner's Tome");
			Tooltip.SetDefault("'Be careful, it's book'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 8;
			Item.shoot = ModContent.ProjectileType<Beginner_Spell>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ItemID.WandofSparking);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Beginner_Spell : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_125";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RubyBolt);
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Dust dust = Main.dust[Terraria.Dust.NewDust(Projectile.Center, 0, 0, DustID.GemRuby, 0f, 0f, 0, new Color(255, 0, 0), 1f)];
			dust.noGravity = true;
			dust.velocity /= 2;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.extraUpdates == 0) {
				return true;
			} else {
				Projectile.extraUpdates = 0;
				if (Projectile.velocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = 0f - oldVelocity.Y;
				}
				if (Projectile.velocity.X != oldVelocity.X) {
					Projectile.velocity.X = 0f - oldVelocity.X;
				}
			}
			return false;
		}
	}
}
