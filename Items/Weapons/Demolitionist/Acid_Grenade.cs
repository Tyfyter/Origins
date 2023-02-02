using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Acid_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Alkaline Grenade");
			Tooltip.SetDefault("Explodes into brine droplets");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 30;
			Item.value *= 14;
			Item.shoot = ModContent.ProjectileType<Acid_Grenade_P>();
			Item.shootSpeed *= 1.5f;
			Item.knockBack = 5f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 3);
			recipe.AddIngredient(ItemID.Grenade, 3);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Acid_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Acid_Grenade";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = 1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
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
			//Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
			int t = ModContent.ProjectileType<Acid_Shot>();
			for (int i = Main.rand.Next(3); i < 6; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 8, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
		}
	}
}
