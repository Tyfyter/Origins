using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace Origins.Projectiles.Weapons {
    public class Lava_Shot : ModProjectile {
        /// <summary>
        /// melee: 1 ranged: 2 magic: 3 summon: 4
        /// </summary>
        public static byte damageType = 0;
        public float frameCount = 15;
        public override string Texture => "Origins/Projectiles/Weapons/Lava_Cast_P";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Magma Shot");
			Main.projFrames[projectile.type] = 2;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Flamelash);
            projectile.light = 0;
            projectile.aiStyle = 1;
            projectile.extraUpdates++;
            projectile.timeLeft = 300;
            switch (damageType) {
                case 1:
                projectile.melee = true;
                break;
                case 2:
                projectile.ranged = true;
                break;
                case 3:
                projectile.magic = true;
                break;
                case 4:
                projectile.minion = true;
                break;
            }
        }
        public override void AI() {
            Lighting.AddLight(projectile.Center, 0.75f, 0.35f, 0f);
			if (++projectile.frameCounter >= (int)frameCount) {
				projectile.frameCounter = 0;
				if (++projectile.frame >= 2) {
					projectile.frame = 0;
				}
			}
            if (projectile.wet) {
                projectile.timeLeft--;
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 34);
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.OnFire, crit?600:300);
            target.AddBuff(BuffID.Oiled, crit?60:30);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            //
            //float alpha = (float)Math.Pow(projectile.frameCounter/frameCount, 0.5f);
            //float alpha = projectile.frameCounter<=3 ? 0.5f : 1;
            bool m = projectile.frameCounter<=3;
            Texture2D texture = Main.projectileTexture[projectile.type];//0f/projectile.frameCounter
            bool flip = projectile.velocity.X < 0;
            float fade = (float)Math.Sqrt(projectile.timeLeft / 300f);
            //projectile.frame^1: bitwise XOR with 1 to use the other frame's height
            if (m)spriteBatch.Draw(texture, projectile.Center-Main.screenPosition, new Rectangle(0, (projectile.frame^1)*26, 56, 26), new Color(1f,1f,1f,0.5f) * fade, projectile.rotation-MathHelper.PiOver2, new Vector2(44, flip ? 13 : 10), projectile.scale, flip ? SpriteEffects.FlipVertically:SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, projectile.Center-Main.screenPosition, new Rectangle(0, projectile.frame*26, 56, 26), new Color(1f,1f,1f,m?0.5f:1f) * fade, projectile.rotation-MathHelper.PiOver2, new Vector2(44, flip ? 13 : 10), projectile.scale, flip ? SpriteEffects.FlipVertically:SpriteEffects.None, 0f);
            return false;
        }
    }
}
