using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Impact_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Grenade");
			Tooltip.SetDefault("Be careful, it's not book");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Grenade);
            Item.damage = 38;
			Item.value*=2;
			Item.useTime = (int)(Item.useTime*0.75);
			Item.useAnimation = (int)(Item.useAnimation*0.75);
            Item.shoot = ModContent.ProjectileType<Impact_Grenade_P>();
			Item.shootSpeed*=1.75f;
            Item.knockBack = 10f;
            Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.Green;
		}
    }
    public class Impact_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Grenade";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Grenade");
            //Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.DamageType = DamageClasses.Explosive;
            Projectile.timeLeft = 135;
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.Grenade;
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
