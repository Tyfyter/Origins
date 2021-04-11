using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Impact_Dynamite : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Impact Dynamite");
			Tooltip.SetDefault("Be careful, it's not book");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Dynamite);
            item.damage = 78;
			item.value*=2;
			item.useTime = (int)(item.useTime*0.75);
			item.useAnimation = (int)(item.useAnimation*0.75);
            item.shoot = ModContent.ProjectileType<Impact_Dynamite_P>();
			item.shootSpeed*=1.75f;
            item.knockBack = 16f;
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Impact_Dynamite_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Dynamite";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Dynamite);
            projectile.penetrate = 1;
            projectile.timeLeft = 225;
        }
        public override void AI() {
            if(projectile.timeLeft<150 && Main.rand.Next(0, projectile.timeLeft) <= 1) projectile.Kill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Dynamite;
            return true;
        }
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 200;
			projectile.height = 200;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
        }
    }
}
