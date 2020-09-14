using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Felnum;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Explosives {
	public class Thermite_Launcher : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Launcher");
			Tooltip.SetDefault("Burn.");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.GrenadeLauncher);
            //item.maxStack = 999;
            item.width = 44;
            item.height = 18;
            item.damage = 34;
			item.value/=2;
			item.useTime = (int)(item.useTime*1.35);
			item.useAnimation = (int)(item.useAnimation*1.35);
            item.shoot = ModContent.ProjectileType<Thermite_P>();
            item.useAmmo = 0;
            item.knockBack = 2f;
            item.shootSpeed = 9f;
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Thermite_P  : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite");
		}
		public override void SetDefaults() {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 18;
            projectile.timeLeft = 900;
            projectile.aiStyle = 14;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 7;
		}
        public override void AI() {
            if(projectile.wet) {
			    Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 14);
                int p = Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Awe_Grenade_Blast>(), (int)(projectile.damage*0.75f), 24, projectile.owner);
                Main.projectile[p].Name = projectile.Name;
                Main.projectile[p].scale = 0.3f;
                Main.projectile[p].extraUpdates = 1;
                projectile.Kill();
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(450, 601));
        }
        public override void OnHitPvp(Player target, int damage, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(450, 601));
        }
        public override string Texture => "Origins/Projectiles/Pixel";
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Dust.NewDust(projectile.Center, 0, 0, 6, Scale:2);
            Dust.NewDust(projectile.position, 16, 16, 6);
            Dust.NewDust(projectile.position, 16, 16, 6);
            if(projectile.timeLeft*projectile.penetrate<5400)return false;
            Dust.NewDust(projectile.Center, 0, 0, 6, Scale:2);
            Dust.NewDust(projectile.position, 16, 16, 6);
            Dust.NewDust(projectile.position, 16, 16, 6);
            if(projectile.timeLeft*projectile.penetrate<10800)return false;
            Dust.NewDust(projectile.Center, 0, 0, 6, Scale:2);
            Dust.NewDust(projectile.position, 16, 16, 6);
            Dust.NewDust(projectile.position, 16, 16, 6);
            return false;
        }
    }
}
