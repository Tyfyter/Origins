using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Bang_Snap : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bang Snap");
			// Tooltip.SetDefault("The Party Girl loves these");
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.damage = 10;
			Item.CountsAsClass(DamageClasses.Explosive);
			Item.shoot = ModContent.ProjectileType<Bang_Snap_P>();
			Item.shootSpeed = 12;
            Item.knockBack = 0;
			Item.value = Item.sellPrice(copper: 1);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 150);
			recipe.AddIngredient(ItemID.SandBlock);
			recipe.AddIngredient(ItemID.SilverOre);
			recipe.Register();
		}
	}
	public class Bang_Snap_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Bang_Snap_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Projectile.penetrate = 1;
		}
		public override void Kill(int timeLeft) {
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
