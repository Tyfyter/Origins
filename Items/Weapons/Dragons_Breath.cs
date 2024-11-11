using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using Origins.Dev;
using System;
using PegasusLib;

namespace Origins.Items.Weapons {
	public class Dragons_Breath : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "ReworkExpected"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.damage = 40;
			Item.crit = 11;
			Item.useAnimation = 43;
			Item.useTime = 43;
			Item.width = 68;
			Item.height = 34;
			Item.useAmmo = ItemID.Fireblossom;
			Item.shoot = ModContent.ProjectileType<Dragons_Breath_P>();
			Item.shootSpeed = 10f;
			Item.knockBack = 2.5f;
			Item.useAmmo = AmmoID.None;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8, 2);
		}
	}
	public class Dragons_Breath_P : ModProjectile {
		public override string Texture => "Terraria/Images/Item_37";
		List<Particle> particles;
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.DamageType = DamageClasses.RangedMagic;
			Projectile.aiStyle = 0;
			Projectile.ignoreWater = false;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = -1;
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.timeLeft = 400;
			particles = new List<Particle> { };
		}
		public override void AI() {
			if (Projectile.timeLeft % 3 == 0) {
				float n = Main.rand.Next(4) * MathHelper.PiOver2;
				particles.Add(new Particle(new PolarVec2(0, Projectile.velocity.ToRotation() + n), Projectile.timeLeft % 60 == 0 ? 1 : -1));
			}
			if (Projectile.timeLeft < 3 && particles.Count > 0) Projectile.timeLeft = 2;
			Vector2 center = Projectile.Center;
			Projectile.oldPosition = center;
			if (Projectile.timeLeft > 3) {
				Dust.NewDustPerfect(center, 6, Vector2.Zero, Scale: Projectile.timeLeft > 72 ? 2 : (Projectile.timeLeft / 36f)).noGravity = true;
			}

			Stack<int> dead = new Stack<int>();
			Particle particle;
			int age;
			int sign;
			bool lowParticleCount = false;
			if (Main.player[Projectile.owner].ownedProjectileCounts[Projectile.type] > 3) lowParticleCount = true;
			for (Projectile.frameCounter = 0; Projectile.frameCounter < particles.Count; Projectile.frameCounter++) {
				particle = particles[Projectile.frameCounter];
				sign = particle.Age > 0 ? 1 : -1;
				age = particle.Age * sign;
				if (age < 30) {
					particle.Pos.R += Main.rand.NextFloat(2f, 2.5f) / (age * 0.0333f + 1);
				} else {
					particle.Pos.R -= 0.1f;
					if (particle.Pos.R <= 0) {
						dead.Push(Projectile.frameCounter);
						continue;
					}
				}
				Projectile.Center = center + (Vector2)particle.Pos;
				Projectile.Damage();
				if (!lowParticleCount || ((Projectile.frameCounter + Projectile.timeLeft) % 3 != 0)) Dust.NewDustPerfect(center + (Vector2)particle.Pos, 6, Vector2.Zero).noGravity = true;
				particle.Age += sign;
				particle.Pos.Theta += sign * 0.1f - ((50 - particle.Pos.R) / 250);
			}
			while (dead.Count > 0) particles.RemoveAt(dead.Pop());
			Projectile.frameCounter = 0;

			Projectile.Center = center;
			Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.05f);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.timeLeft /= 2;
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Projectile.frame = 0;
			if (Projectile.velocity == Vector2.Zero) modifiers.HitDirectionOverride = target.Center.X > Projectile.Center.X ? -1 : 1;
			else {
				Vector2 diff = (target.Center - (Projectile.Center + Projectile.velocity * 2)).SafeNormalize(default);
				float dot = Vector2.Dot(diff, Vector2.Normalize(Projectile.velocity));
				if (dot < 0) {
					Projectile.frame = 1;
					target.oldVelocity = target.velocity;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			bool centered = target.Hitbox.Contains(Projectile.oldPosition.ToPoint());
			target.AddBuff(BuffID.Daybreak, centered ? 30 : 5);
			target.immune[Projectile.owner] = 12;
			if (Projectile.timeLeft < 3 || centered) Projectile.velocity *= 0.95f;
			if (Projectile.frame == 1) {
				PolarVec2 vel = particles[Projectile.frameCounter].Pos;
				vel.R = hit.Knockback * -1.5f;
				if (hit.Crit) vel.R *= 2;
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
