using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Impact_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Bomb");
			Tooltip.SetDefault("Be careful, it's not book");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Bomb);
            item.damage = 52;
			item.value*=2;
			item.useTime = (int)(item.useTime*0.75);
			item.useAnimation = (int)(item.useAnimation*0.75);
            item.shoot = ModContent.ProjectileType<Impact_Bomb_P>();
			item.shootSpeed*=2;
            item.knockBack = 13f;
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Impact_Bomb_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Bomb";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bomb);
            projectile.penetrate = 1;
            projectile.timeLeft = 135;
        }
        public override void AI() {
            if(projectile.timeLeft<60 && Main.rand.Next(0, projectile.timeLeft) == 0) projectile.Kill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.Kill();
            return false;
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
        }
    }
}
