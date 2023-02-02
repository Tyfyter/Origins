using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Impact_Dynamite : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Dynamite");
			Tooltip.SetDefault("'Be careful, it's not a book'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 75;
			Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);
			Item.shoot = ModContent.ProjectileType<Impact_Dynamite_P>();
			Item.shootSpeed *= 1.75f;
			Item.knockBack = 16f;
			Item.value = Item.sellPrice(silver: 6);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Dynamite);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Impact_Dynamite_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Impact_Dynamite";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Dynamite");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 225;
		}
		public override void AI() {
			if (Projectile.timeLeft < 150 && Main.rand.Next(0, Projectile.timeLeft) <= 1) Projectile.Kill();
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Dynamite;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 200;
			Projectile.height = 200;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
