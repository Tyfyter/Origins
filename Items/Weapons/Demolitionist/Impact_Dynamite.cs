using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Impact_Dynamite : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsDynamite",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
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
			Item.rare = ItemRarityID.Green;
            Item.ArmorPenetration += 3;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 3);
			recipe.AddIngredient(ItemID.Dynamite, 3);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss_Item>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Impact_Dynamite_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Impact_Dynamite";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
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
		public override void OnKill(int timeLeft) {
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
