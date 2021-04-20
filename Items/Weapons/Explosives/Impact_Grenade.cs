using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Impact_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Grenade");
			Tooltip.SetDefault("Be careful, it's not book");
            Origins.ExplosiveItems[item.type] = true;
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Grenade);
            item.damage = 38;
			item.value*=2;
			item.useTime = (int)(item.useTime*0.75);
			item.useAnimation = (int)(item.useAnimation*0.75);
            item.shoot = ModContent.ProjectileType<Impact_Grenade_P>();
			item.shootSpeed*=1.75f;
            item.knockBack = 10f;
            item.ammo = ItemID.Grenade;
			item.rare = ItemRarityID.Green;
		}
    }
    public class Impact_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Grenade";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Grenade");
            Origins.ExplosiveProjectiles[projectile.type] = true;
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.timeLeft = 135;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
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
        }
    }
}
