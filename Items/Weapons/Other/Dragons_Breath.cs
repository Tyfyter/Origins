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
            item.damage = 50;
            item.crit = 11;
            item.useAnimation = 23;
            item.useTime = 23;
            item.width = 78;
            item.height = 34;
            item.shoot = ModContent.ProjectileType<Dragons_Breath_P>();
            item.shootSpeed = 2.5f;
            item.useAmmo = AmmoID.None;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-16,2);
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
            projectile.timeLeft -= projectile.timeLeft/3;
            particles = new List<Particle>{};
        }
        public override void AI() {
            projectile.ignoreWater = true;
            if(projectile.timeLeft%10==0) {
                particles.Add(new Particle(new PolarVec2(0, projectile.velocity.ToRotation()), projectile.timeLeft%60==0?1:-1));
            }
            if(projectile.timeLeft < 10 && particles.Count>0)projectile.timeLeft = 9;
            Vector2 center = projectile.Center;
            projectile.oldPosition = center;
            if(projectile.timeLeft > 10)Dust.NewDustPerfect(center, 6, Vector2.Zero, Scale:1.5f).noGravity = true;

            Stack<int> dead = new Stack<int>();
            Particle particle;
            int age;
            int sign;
            for(int i = 0; i < particles.Count; i++) {
                particle = particles[i];
                sign = particle.Age > 0 ? 1 : -1;
                age = particle.Age * sign;
                if(age < 30) {
                    particle.Pos.R += Main.rand.NextFloat(2f, 2.5f)/(age*0.0333f+1);
                } else {
                    particle.Pos.R -= 0.1f;
                    if(particle.Pos.R<=0) {
                        dead.Push(i);
                        continue;
                    }
                }
                projectile.Center = center + (Vector2)particle.Pos;
                projectile.Damage();
                Dust.NewDustPerfect(center+(Vector2)particle.Pos, 6, Vector2.Zero).noGravity = true;
                particle.Age += sign;
                particle.Pos.Theta += sign * 0.1f - ((50 - particle.Pos.R)/250);
            }
            while(dead.Count>0)particles.RemoveAt(dead.Pop());

            projectile.Center = center;
            projectile.ignoreWater = false;
	        Lighting.AddLight(projectile.Center, 0.5f, 0.25f, 0.05f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.tileCollide = false;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft /= 2;
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Daybreak, projectile.ignoreWater?5:30);
			//target.immune[projectile.owner] = 5;
            if(projectile.timeLeft < 10 || target.Hitbox.Contains(projectile.oldPosition.ToPoint()))projectile.velocity*=0.95f;
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
