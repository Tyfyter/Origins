using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Chlorodynamite : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chlorodynamite");
			Tooltip.SetDefault("Vines will pull nearby enemies in before detonation");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 186;
			Item.shoot = ModContent.ProjectileType<Chlorodynamite_P>();
			Item.shootSpeed *= 1.5f;
			Item.value = Item.sellPrice(silver: 22);
			Item.rare = ItemRarityID.Lime;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.ChlorophyteBar);
			recipe.AddIngredient(ItemID.Dynamite, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.ChlorophyteOre, 5);
			recipe.AddIngredient(ItemID.Dynamite, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Chlorodynamite_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Chlorodynamite";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chlorodynamite");
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
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
