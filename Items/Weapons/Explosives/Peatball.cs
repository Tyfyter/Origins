using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Peatball : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Peat Ball");
			Tooltip.SetDefault("Peat Ball");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Snowball);
            //item.maxStack = 999;
            item.damage*=3;
			item.value+=20;
			item.useTime = (int)(item.useTime*0.75);
			item.useAnimation = (int)(item.useAnimation*0.75);
            item.shoot = ModContent.ProjectileType<Peatball_P>();
			item.shootSpeed*=1.35f;
            item.knockBack*=2;
			item.rare = ItemRarityID.Blue;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Peatball_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Peatball";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
            projectile.penetrate = 1;
            projectile.width = 12;
            projectile.height = 12;
            projectile.scale = 0.85f;
        }
        /*public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }*/
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 48;
			projectile.height = 48;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 14, 0.66f);
            Main.gore[Gore.NewGore(new Vector2(projectile.Center.X, projectile.Center.Y), default, Main.rand.Next(61, 64))].velocity += Vector2.One;
            Main.gore[Gore.NewGore(new Vector2(projectile.Center.X, projectile.Center.Y), default, Main.rand.Next(61, 64))].velocity += Vector2.One;
            //Main.gore[Gore.NewGore(new Vector2(projectile.Center.X, projectile.Center.Y), default, Main.rand.Next(61, 64))].velocity += Vector2.One;
        }
    }
}
