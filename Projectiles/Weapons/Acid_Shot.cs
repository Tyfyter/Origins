using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Weapons {
    public class Acid_Shot : ModProjectile, IElementalProjectile {
        public ushort Element => Elements.Acid;
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
            projectile.extraUpdates = 1;
            projectile.width = projectile.height = 10;
            projectile.light = 0;
            projectile.timeLeft = 180;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 20;
        }
        public override void AI() {
            if(projectile.ai[1]<=0/*projectile.timeLeft<168*/) {
                Lighting.AddLight(projectile.Center, 0, 0.75f*projectile.scale, 0.3f*projectile.scale);
                if(projectile.timeLeft%3==0) {
                    Dust dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity * -0.25f, 100, new Color(0, 255, 0), projectile.scale);
                    dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                    dust.noGravity = false;
                    dust.noLight = true;
                }
            } else {
			    projectile.Center = Main.player[projectile.owner].itemLocation+projectile.velocity;
                projectile.ai[1]--;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.timeLeft>168&&(projectile.ai[1]%1+1)%1==0.5f) {
                projectile.velocity-=oldVelocity-projectile.velocity;
                return false;
            }
            return true;
        }
        public override void Kill(int timeLeft) {
            for(int i = 0; i < 7; i++) {
                Dust dust = Dust.NewDustDirect(projectile.position, 10, 10, 226, 0, 0, 100, new Color(0, 255, 0), 1.25f*projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = (int)(96*projectile.scale);
			projectile.height = (int)(96*projectile.scale);
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
            projectile.damage = (int)(projectile.damage*0.75f);
			projectile.Damage();
            Main.PlaySound(SoundID.Item10, projectile.position);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(projectile.timeLeft>168&&(projectile.ai[1]%1+1)%1==0.5f)projectile.penetrate++;
            target.AddBuff(ModContent.BuffType<Solvent_Debuff>(), 480);
            target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f*projectile.scale);
            dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
            dust.noGravity = false;
            dust.noLight = true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
}