using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Impact_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Bomb");
			Tooltip.SetDefault("'Be careful, it's not a book'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 45;
			Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);
			Item.shoot = ModContent.ProjectileType<Impact_Bomb_P>();
			Item.shootSpeed *= 1.75f;
			Item.knockBack = 13f;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Bomb, 2);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Impact_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Impact_Bomb";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Bomb");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
		}
		public override void AI() {
			if (Projectile.timeLeft < 60 && Main.rand.Next(0, Projectile.timeLeft) == 0) Projectile.Kill();
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
