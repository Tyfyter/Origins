using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using static Origins.OriginExtensions;

namespace Origins.Projectiles.Weapons {
    public class Cryostrike_P : ModProjectile {
        public static float margin = 0.5f;
        float stabVel = 1;
        float drawOffsetY;
        public override string Texture => "Origins/Projectiles/Weapons/Icicle_P";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);//sets the projectile stat values to those of Ruby Bolts
            projectile.ranged = false;
            projectile.magic = true;
            projectile.penetrate = -1;//when projectile.penetrate reaches 0 the projectile is destroyed
            projectile.extraUpdates = 1;
            projectile.aiStyle = 1;
            projectile.localNPCHitCooldown = 10;
            drawOffsetY = 0;//-34;
            drawOriginOffsetX = -0.5f;
            projectile.hide = true;
        }
        public override void AI() {
            if(projectile.aiStyle == 0) {
                if(drawOffsetY > -20) {
                    drawOffsetY-=stabVel;
                } else {
                    drawOffsetY = -20;
                }
                projectile.velocity = Vector2.Zero;
            }
            drawOriginOffsetY = (int)drawOffsetY;
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.aiStyle != 0) {
                projectile.aiStyle = 0;
                projectile.knockBack = 0.1f;
                projectile.timeLeft = 180;
                projectile.usesLocalNPCImmunity = true;
                stabVel = projectile.velocity.Length()/2;//(oldVelocity-projectile.velocity).Length();
            }
            return false;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            if(projectile.aiStyle == 0) {
                hitbox = hitbox.Add(new Vector2(0,-1.5f).RotatedBy(projectile.rotation)*(-34-drawOffsetY));
            }
        }
        public override bool? CanHitNPC(NPC target) {
            return (projectile.aiStyle!=0||PokeAngle(target.velocity))?null:new bool?(false);
        }
        bool PokeAngle(Vector2 velocity) {
            return NormDot(velocity, Vec2FromPolar(projectile.rotation-MathHelper.PiOver2))>1f-margin;
        }
    }
}
