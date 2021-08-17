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

namespace Origins.Projectiles.Misc {
    public class Felnum_Shock_Leader : ModProjectile {
        public static int ID { get; private set; }
        public Entity Parent { get; internal set; }
        public event Action OnStrike;
        public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Shock");
            ID = projectile.type;
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.aiStyle = 0;
            projectile.timeLeft = 20;
            projectile.extraUpdates = 19;
            projectile.width = projectile.height = 2;
            projectile.penetrate = 1;
            projectile.light = 0;
            projectile.usesLocalNPCImmunity = true;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            hitbox.X -= 2;
            hitbox.Y -= 2;
            hitbox.Width += 4;
            hitbox.Height += 4;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage += target.defense / 2;
        }
        public override void Kill(int timeLeft) {
            if(timeLeft>0) {
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 60, 0.65f, 1f);
                Projectile proj = Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, Felnum_Shock_Arc.ID, 0, 0, projectile.owner, projectile.ai[0], projectile.ai[1]);
                if(proj.modProjectile is Felnum_Shock_Arc shock) {
                    shock.Parent = Parent;
                }
                if(!(OnStrike is null))OnStrike();
            }
        }
    }
    public class Felnum_Shock_Arc  : ModProjectile {
        public static int ID { get; private set; }
        public Entity Parent { get; internal set; }
        public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetStaticDefaults() {
            ID = projectile.type;
		}
        public override void SetDefaults() {
            projectile.timeLeft = 10;
            projectile.width = projectile.height = 0;
            projectile.penetrate = -1;
            projectile.hide = true;
            projectile.localAI[0] = (float)Math.Pow(Main.rand.NextFloat(-4, 4), 2);
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsOverWiresUI.Add(index);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            Vector2 arcStart = (Parent?.position??Vector2.Zero) + new Vector2(projectile.ai[0], projectile.ai[1]);
            spriteBatch.DrawLightningArcBetween(
                arcStart - Main.screenPosition,
                projectile.position - Main.screenPosition,
                projectile.localAI[0]);
            //projectile.localAI[0] *= 0.8f;
            for(int i = 0; i < 16; i++) {
                Lighting.AddLight(Vector2.Lerp(projectile.Center, arcStart, i/16f), 0.15f, 0.4f, 0.43f);
            }
            return false;
        }
    }
}
