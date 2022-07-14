using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Beginner_Tome : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Beginner's Tome");
			Tooltip.SetDefault("Be careful, it's book");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 8;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Beginner_Spell>();
			Item.rare = ItemRarityID.Green;
		}
    }
    public class Beginner_Spell : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_125";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.RubyBolt);//sets the projectile stat values to those of Ruby Bolts
            Projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
            Projectile.extraUpdates = 1;
        }
        public override void AI() {
	        Dust dust = Main.dust[Terraria.Dust.NewDust(Projectile.Center, 0, 0, DustID.GemRuby, 0f, 0f, 0, new Color(255,0,0), 1f)];
	        dust.noGravity = true;
	        dust.velocity/=2;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.extraUpdates == 0){
                return true;
			}else{
                Projectile.extraUpdates = 0;
				if (Projectile.velocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = 0f - oldVelocity.Y;
				}
				if (Projectile.velocity.X != oldVelocity.X) {
					Projectile.velocity.X = 0f - oldVelocity.X;
				}
			}
            return false;
        }
    }
}
