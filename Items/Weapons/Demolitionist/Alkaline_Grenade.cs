using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Alkaline_Grenade : ModItem {
        public string[] Categories => new string[] {
            "ThrownExplosive",
			"IsGrenade"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 60;
			Item.value = 500;
			Item.shoot = ModContent.ProjectileType<Acid_Grenade_P>();
			Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.LightRed;
			Item.maxStack = 999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ItemID.Grenade, 8);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Acid_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Alkaline_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = 1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			//Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
			int t = ModContent.ProjectileType<Acid_Shot>();
			for (int i = Main.rand.Next(3); i < 5; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 8, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
		}
	}
}
