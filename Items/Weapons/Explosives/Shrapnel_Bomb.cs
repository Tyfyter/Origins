using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Shrapnel_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shrapnel Bomb");
			Tooltip.SetDefault("Explodes into shrapnel");
            Origins.ExplosiveItems[item.type] = true;
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Bomb);
            item.damage = 89;
			item.value*=2;
			item.useTime = (int)(item.useTime*1.15);
			item.useAnimation = (int)(item.useAnimation*1.15);
            item.shoot = ModContent.ProjectileType<Shrapnel_Bomb_P>();
			item.shootSpeed*=0.95f;
            item.knockBack = 13f;
			item.rare = ItemRarityID.Green;
		}
    }
    public class Shrapnel_Bomb_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Shrapnel_Bomb";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shrapnel Bomb");
            Origins.ExplosiveProjectiles[projectile.type] = true;
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bomb);
            projectile.penetrate = 1;
            projectile.timeLeft = 135;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Bomb;
            return true;
        }
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
            int center = Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ace_Shrapnel_P>(), projectile.damage, projectile.knockBack, projectile.owner);
            Vector2 v;
            for(int i = 4; i-->0;) {
                v = Main.rand.NextVector2Unit()*6;
                Projectile.NewProjectile(projectile.Center+v*8, v, ModContent.ProjectileType<Ace_Shrapnel_Shard>(), projectile.damage/2, projectile.knockBack/4, projectile.owner, center, 4);
            }
        }
    }
}
