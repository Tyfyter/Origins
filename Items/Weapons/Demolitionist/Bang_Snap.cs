using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Bang_Snap : ModItem {
        public string[] Categories => new string[] {
            "ThrownExplosive"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.damage = 10;
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.shoot = ModContent.ProjectileType<Bang_Snap_P>();
			Item.shootSpeed = 12;
            Item.knockBack = 0;
			Item.value = Item.sellPrice(copper: 1);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 150);
			recipe.AddIngredient(ItemID.SilverOre);
            recipe.AddIngredient(ItemID.SandBlock);
            recipe.Register();

            recipe = Recipe.Create(Type, 150);
            recipe.AddIngredient(ItemID.SilverOre);
            recipe.AddIngredient(ItemID.EbonsandBlock);
            recipe.Register();

            recipe = Recipe.Create(Type, 150);
            recipe.AddIngredient(ItemID.SilverOre);
            recipe.AddIngredient(ItemID.CrimsandBlock);
            recipe.Register();

            recipe = Recipe.Create(Type, 150);
            recipe.AddIngredient(ItemID.SilverOre);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Sand_Item>());
            recipe.Register();

            recipe = Recipe.Create(Type, 150);
            recipe.AddIngredient(ItemID.SilverOre);
            recipe.AddIngredient(ModContent.ItemType<Silica_Item>());
            recipe.Register();

            /*recipe = Recipe.Create(Type, 150);
            recipe.AddIngredient(ItemID.SilverOre);
            recipe.AddIngredient(ModContent.ItemType<Ashen_Sand_Item>());
            recipe.Register();*/
        }
	}
	public class Bang_Snap_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Bang_Snap_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ThrownExplosive;
			Projectile.penetrate = 1;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item40.WithPitch(2f).WithVolume(1f), Projectile.Center);
		}
	}
}
