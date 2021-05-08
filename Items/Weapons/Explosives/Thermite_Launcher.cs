using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Ammo;
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
			Tooltip.SetDefault("Burn.\nUses Thermite Canisters for ammo");
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
            item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
            item.useAmmo = ModContent.ItemType<Thermite_Canister>();
            item.knockBack = 2f;
            item.shootSpeed = 12f;
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            type = item.shoot;
            return true;
        }
    }
    public class Thermite_Canister_P  : ModProjectile {
        public override string Texture => "Origins/Projectiles/Ammo/Thermite_Canister_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Canister");
		}
		public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 900;
            //projectile.aiStyle = 14;
            //projectile.usesLocalNPCImmunity = true;
            //projectile.localNPCHitCooldown = 7;
		}
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.velocity.X==0f) {
                projectile.velocity.X = -oldVelocity.X;
            }
            if(projectile.velocity.Y==0f) {
                projectile.velocity.Y = -oldVelocity.Y;
            }
            projectile.timeLeft = 1;
            return true;
        }
        public override void Kill(int timeLeft) {
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 0, 0, projectile.owner, -1, 1);
            projectile.damage = (int)(projectile.damage * 0.75f);
            projectile.knockBack = 16f;
		    projectile.position = projectile.Center;
		    projectile.width = (projectile.height = 52);
		    projectile.Center = projectile.position;
            projectile.Damage();
            for(int i = 0; i < 5; i++) {
                Projectile.NewProjectile(projectile.Center, (projectile.velocity/2)+Vec2FromPolar((i/Main.rand.NextFloat(5,7))*MathHelper.TwoPi, Main.rand.NextFloat(2,4)), ModContent.ProjectileType<Thermite_P>(), (int)(projectile.damage*0.75f), 0, projectile.owner);
            }
        }
        public override void AI() {
            Dust.NewDust(projectile.Center, 0, 0, 6);
        }
    }
    public class Thermite_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Ammo/Napalm_Pellet_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Canister");
            Origins.ExplosiveProjectiles[projectile.type] = true;
		}
        public override void SetDefaults() {
            projectile.friendly = true;
            projectile.width = 6;
            projectile.height = 6;
            projectile.aiStyle = 1;
            projectile.penetrate = 25;
            projectile.timeLeft = Main.rand.Next(300, 451);
        }
        public override void AI() {
			float v = 0.75f+(float)(0.125f*(Math.Sin(projectile.timeLeft/5f)+2*Math.Sin(projectile.timeLeft/60f)));
            Lighting.AddLight(projectile.Center, v,v*0.5f,0);
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = height = 0;
            fallThrough = true;
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.ai[0]==0f) {
                projectile.ai[0] = 1f;
                projectile.aiStyle = 0;
                projectile.tileCollide = false;
                projectile.position+=Vector2.Normalize(oldVelocity)*2;
            }
            projectile.velocity = Vector2.Zero;
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
        }
        public override void OnHitPvp(Player target, int damage, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
        }
        public override Color? GetAlpha(Color lightColor) {
			int v = 200+(int)(25*(Math.Sin(projectile.timeLeft/5f)+Math.Sin(projectile.timeLeft/60f)));
            return new Color(v+20,v+25,v-150,0);
        }
    }
}
