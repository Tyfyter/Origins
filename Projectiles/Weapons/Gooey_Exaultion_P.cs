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
    public class Gooey_Exaultion_P : ModProjectile, IElementalProjectile {
        public float frameCount = 15;
        public ushort Element => Elements.Acid;
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
            Lighting.AddLight(projectile.Center, 0, 0.75f * projectile.scale, 0.35f * projectile.scale);
            if (++projectile.frameCounter >= (int)frameCount) {
                projectile.frameCounter = 0;
                if (++projectile.frame >= 2) {
                    projectile.frame = 0;
                }
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
            target.AddBuff(BuffID.OgreSpit, 480);
            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f*projectile.scale);
            dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
            dust.noGravity = false;
            dust.noLight = true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //
            //float alpha = (float)Math.Pow(projectile.frameCounter/frameCount, 0.5f);
            //float alpha = projectile.frameCounter<=3 ? 0.5f : 1;
            bool m = false;//projectile.frameCounter <= 3;
            Texture2D texture = Main.projectileTexture[projectile.type];//0f/projectile.frameCounter
            bool flip = projectile.velocity.X < 0;
            float fade = (float)Math.Sqrt(projectile.timeLeft / 300f);
            //projectile.frame^1: bitwise XOR with 1 to use the other frame's height
            //if (m) spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, new Rectangle(0, (projectile.frame ^ 1) * 14, 34, 34), new Color(1f, 1f, 1f, 0.5f) * fade, projectile.rotation - MathHelper.PiOver2, new Vector2(26, 7), projectile.scale, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, new Rectangle(0, projectile.frame * 14, 34, 14), new Color(1f, 1f, 1f, m ? 0.5f : 1f) * fade, projectile.rotation - MathHelper.PiOver2, new Vector2(26, 7), projectile.scale, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            return false;
        }
    }
}