using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Projectiles.Weapons {
	public class Defiled_Spike_Explosion : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public override void SetDefaults() {
			projectile.timeLeft = 600;
            projectile.usesLocalNPCImmunity = true;
            projectile.hide = true;
        }
        public override void AI() {
			if (projectile.ai[0] > 0) {
                projectile.ai[0]--;
                int[] immune = projectile.localNPCImmunity.ToArray();
                Projectile proj = Projectile.NewProjectileDirect(projectile.Center,
                    (Vector2)new PolarVec2(Main.rand.NextFloat(8, 16), projectile.ai[1]++),
                    Defiled_Spike_Explosion_Spike.ID,
                    projectile.damage,
                    0,
                    projectile.owner,
                    ai1:projectile.whoAmI
                );
                for (int i = 0; i < 200; i++) { // for some reason spawning the spikes 
                    if (immune[i] != projectile.localNPCImmunity[i]) {
                        projectile.localNPCImmunity[i] = immune[i];
                    }
                }
                proj.localNPCImmunity = projectile.localNPCImmunity;
                //localNPCImmunity is never overwritten in vanilla, and since it's an array I can just do this to permanently link the cooldowns of two projectiles
            }
        }
    }
	public class Defiled_Spike_Explosion_Spike : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Spike Eruption");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.timeLeft = Main.rand.Next(22, 25);
            projectile.width = 18;
            projectile.height = 18;
            projectile.aiStyle = 0;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.usesLocalNPCImmunity = false;
            projectile.hide = true;
        }
        public float movementFactor {
            get => projectile.ai[0];
            set => projectile.ai[0] = value;
        }
        public override void AI() {
            Projectile projOwner = Main.projectile[(int)projectile.ai[1]];
            projectile.Center = projOwner.Center - projectile.velocity;
            if (movementFactor == 0f) {
                movementFactor = 1f;
                //if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
                projectile.netUpdate = true;
            }
            if (projectile.timeLeft > 18) {
                movementFactor += 1f;
            }
            projectile.position += projectile.velocity * movementFactor;
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.rotation += MathHelper.PiOver2;
            projOwner.timeLeft = 7;
        }
		public override bool? CanHitNPC(NPC target) {
			if (target.Hitbox.Intersects(projectile.Hitbox)) {

			}
			if (projectile.localNPCImmunity[target.whoAmI] == 0) {
                return null;
			}
			return false;
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            projectile.localNPCImmunity[target.whoAmI] = 35*7;
            target.immune[projectile.owner] = 0;
        }
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            float totalLength = projectile.velocity.Length() * movementFactor;
            int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
            lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, projectile.rotation, new Vector2(9, 0), projectile.scale, SpriteEffects.None, 0);
            totalLength -= 58;
            Vector2 offset = projectile.velocity.SafeNormalize(Vector2.Zero) * 58;
            Texture2D texture = mod.GetTexture("Projectiles/Weapons/Dismay_Mid");
            int c = 0;
            Vector2 pos;
            for (int i = (int)totalLength; i > 0; i -= 58) {
                c++;
                pos = (projectile.Center - Main.screenPosition) - (offset * c);
                //lightColor = new Color(Lighting.GetSubLight(pos));//projectile.GetAlpha(new Color(Lighting.GetSubLight(pos)));
                spriteBatch.Draw(texture, pos, new Rectangle(0, 0, 18, System.Math.Min(58, i)), lightColor, projectile.rotation, new Vector2(9, 0), projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
