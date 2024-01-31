using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Sunflower_Seed_Shooter : ModItem {
        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 6;
			Item.width = 64;
			Item.height = 22;
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.shoot = ModContent.ProjectileType<Sunflower_Seed_P>();
            Item.useAmmo = ItemID.Sunflower;
            Item.knockBack = 1;
			Item.shootSpeed = 9f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.White;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(4);
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
