using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Impact_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Bomb");
			Tooltip.SetDefault("Be careful, it's not book");
            Origins.ExplosiveItems[Item.type] = true;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Bomb);
            Item.damage = 46;
			Item.value*=2;
			Item.useTime = (int)(Item.useTime*0.75);
			Item.useAnimation = (int)(Item.useAnimation*0.75);
            Item.shoot = ModContent.ProjectileType<Impact_Bomb_P>();
			Item.shootSpeed*=1.75f;
            Item.knockBack = 13f;
			Item.rare = ItemRarityID.Green;
		}
    }
    public class Impact_Bomb_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Bomb";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Bomb");
            Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Bomb);
            Projectile.penetrate = 1;
            Projectile.timeLeft = 135;
        }
        public override void AI() {
            if(Projectile.timeLeft<60 && Main.rand.Next(0, Projectile.timeLeft) == 0) Projectile.Kill();
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
