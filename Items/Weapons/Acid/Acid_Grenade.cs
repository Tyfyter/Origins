using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Acid {
	public class Acid_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acid Grenade");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Grenade);
            //item.maxStack = 999;
            item.damage = 30;
			item.value*=14;
            item.shoot = ModContent.ProjectileType<Acid_Grenade_P>();
			item.shootSpeed*=1.5f;
            item.knockBack = 5f;
            item.ammo = ItemID.Grenade;
			item.rare = ItemRarityID.Lime;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Acid_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Acid/Acid_Grenade";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.timeLeft = 135;
            projectile.penetrate = 1;
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
			//Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            int t = ModContent.ProjectileType<Acid_Splash_P>();
            for(int i = Main.rand.Next(2); i < 4; i++)Projectile.NewProjectileDirect(projectile.Center, (Main.rand.NextVector2Unit()*4)+(projectile.velocity/8), t, projectile.damage/8, 6, projectile.owner, ai1:-0.5f).scale = 0.85f;
        }
    }
}
