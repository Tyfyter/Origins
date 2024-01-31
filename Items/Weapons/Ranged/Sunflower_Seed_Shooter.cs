using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Sunflower_Seed_Shooter : ModItem {
        public override void SetStaticDefaults() {
            Origins.FlatDamageMultiplier[Type] = 2f / 8f;
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 10;
			Item.crit -= 4;
			Item.width = 64;
			Item.height = 22;
			Item.useTime = 8;
			Item.useAnimation = 30;
			Item.shoot = ModContent.ProjectileType<Sunflower_Seed_P>();
			Item.useAmmo = ItemID.Sunflower;
            Item.knockBack = 1;
			Item.shootSpeed = 9f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.White;
            Item.reuseDelay = 18;
            Item.autoReuse = true;
            Item.consumeAmmoOnFirstShotOnly = true;
        }
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(3);
		}
    }
	public class Sunflower_Seed_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ranged/Sunflower_Seed_Shooter_P";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.width = 6;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
		}
    }
}
