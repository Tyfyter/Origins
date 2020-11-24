using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Beginner_Tome : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Beginner's Tome");
			Tooltip.SetDefault("Be careful, it's book");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 16;
			item.magic = true;
			item.noMelee = true;
			item.width = 28;
			item.height = 30;
			item.useTime = 20;
			item.useAnimation = 20;
			item.mana = 8;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Beginner_Spell>();
			item.rare = ItemRarityID.Green;
		}
    }
    public class Beginner_Spell : ModProjectile {
        public override string Texture => "Terraria/Projectile_125";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.RubyBolt);//sets the projectile stat values to those of Ruby Bolts
            projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
            projectile.extraUpdates = 1;
        }
        public override void AI() {
	        Dust dust = Main.dust[Terraria.Dust.NewDust(projectile.Center, 0, 0, 90, 0f, 0f, 0, new Color(255,0,0), 1f)];
	        dust.noGravity = true;
	        dust.velocity/=2;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
			if (projectile.extraUpdates == 0){
                return true;
			}else{
                projectile.extraUpdates = 0;
				if (projectile.velocity.Y != oldVelocity.Y) {
					projectile.velocity.Y = 0f - oldVelocity.Y;
				}
				if (projectile.velocity.X != oldVelocity.X) {
					projectile.velocity.X = 0f - oldVelocity.X;
				}
			}
            return false;
        }
    }
}
