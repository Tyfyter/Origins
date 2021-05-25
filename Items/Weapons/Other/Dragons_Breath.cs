using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Dragons_Breath : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dragon's Breath");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.SniperRifle);
            item.damage = 25;
            item.crit = 11;
            item.useAnimation = 43;
            item.useTime = 43;
            item.width = 78;
            item.height = 34;
            item.shoot = ModContent.ProjectileType<Dragons_Breath_P>();
            item.shootSpeed = 2.5f;
            item.knockBack = 2.5f;
            item.useAmmo = AmmoID.None;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8,2);
        }
    }
    public class Dragons_Breath_P : ModProjectile {
        public override string Texture => "Terraria/Item_37";
        List<Particle> particles;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dragon's Breath");
            Origins.ExplosiveProjectiles[projectile.type] = true;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
            projectile.aiStyle = 0;
            projectile.ignoreWater = false;
            projectile.extraUpdates = 0;
            projectile.penetrate = -1;
            projectile.width = 4;
            projectile.height = 4;
            projectile.timeLeft = 400;
            particles = new List<Particle>{};
        }
        public override void AI() {
            //projectile.ignoreWater = true;
            if(projectile.timeLeft%12==0) {
                float n = Main.rand.Next(4)*MathHelper.PiOver2;
                particles.Add(new Particle(new PolarVec2(0, projectile.velocity.ToRotation()+n), projectile.timeLeft%60==0?1:-1));
            }
            if(projectile.timeLeft < 12 && particles.Count>0)projectile.timeLeft = 11;
            Vector2 center = projectile.Center;
            projectile.oldPosition = center;
            if(projectile.timeLeft > 12) {
                Dust.NewDustPerfect(center, 6, Vector2.Zero, Scale: projectile.timeLeft > 72 ? 2 : (projectile.timeLeft / 36f)).noGravity = true;
            }

            Stack<int> dead = new Stack<int>();
            Particle particle;
            int age;
            int sign;
            bool lowParticleCount = false;
            /*int count = Main.player[projectile.owner].ownedProjectileCounts[projectile.type];
            if(count > 2) {
                count -= 2;
                lowParticleCount = projectile.timeLeft > 12;
            }*/
            if(Main.player[projectile.owner].ownedProjectileCounts[projectile.type]>3)lowParticleCount = true;
            for(projectile.frameCounter = 0; projectile.frameCounter < particles.Count; projectile.frameCounter++) {
                particle = particles[projectile.frameCounter];
                sign = particle.Age > 0 ? 1 : -1;
                age = particle.Age * sign;
                if(age < 30) {
                    particle.Pos.R += Main.rand.NextFloat(2f, 2.5f)/(age*0.0333f+1);
                } else {
                    particle.Pos.R -= 0.1f;
                    if(particle.Pos.R<=0) {
                        dead.Push(projectile.frameCounter);
                        continue;
                    }
                }
                projectile.Center = center + (Vector2)particle.Pos;
                projectile.Damage();
                if(!lowParticleCount||((projectile.frameCounter+projectile.timeLeft)%3!=0))Dust.NewDustPerfect(center+(Vector2)particle.Pos, 6, Vector2.Zero).noGravity = true;
                particle.Age += sign;
                particle.Pos.Theta += sign * 0.1f - ((50 - particle.Pos.R)/250);
            }
            while(dead.Count>0)particles.RemoveAt(dead.Pop());
            projectile.frameCounter = 0;

            projectile.Center = center;
            //projectile.ignoreWater = false;
	        Lighting.AddLight(projectile.Center, 0.5f, 0.25f, 0.05f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.tileCollide = false;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft /= 2;
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            projectile.frame = 0;
            if(projectile.velocity==Vector2.Zero)hitDirection = target.Center.X > projectile.Center.X?-1:1;
            else {
                Vector2 diff = (target.Center - (projectile.Center + projectile.velocity*2)).SafeNormalize(default);
                float dot = Vector2.Dot(diff, Vector2.Normalize(projectile.velocity));
                if(dot<0) {
                    projectile.frame = 1;
                    target.oldVelocity = target.velocity;
                }
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            bool centered = target.Hitbox.Contains(projectile.oldPosition.ToPoint());
            target.AddBuff(BuffID.Daybreak, centered?30:5);
			target.immune[projectile.owner] = 12;
            if(projectile.timeLeft < 12 || centered)projectile.velocity*=0.95f;
            if(projectile.frame == 1) {
                PolarVec2 vel = particles[projectile.frameCounter].Pos;
                vel.R = knockback*-1.5f;
                if(crit)vel.R *= 2;
                target.velocity = Vector2.Lerp(target.velocity, (Vector2)vel, target.knockBackResist);
            }
        }
        class Particle {
            public PolarVec2 Pos;
            public int Age;
            public Particle(PolarVec2 pos, int age) {
                Pos = pos;
                Age = age;
            }
            public Particle(int age) {
                Age = age;
                Pos = default;
            }
        }
    }
}
