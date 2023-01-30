using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class ExplosiveGlobalProjectile : GlobalProjectile {
		bool isHoming = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.DamageType.CountsAsClass(DamageClasses.Explosive) || GetVanillaExplosiveType(entity) > 0;
		}
		public override void SetDefaults(Projectile projectile) {
			isHoming = false;
		}
		public override void AI(Projectile projectile) {
			if (isHoming && !projectile.minion) {
				float targetWeight = 300;
				Vector2 targetPos = default;
				bool foundTarget = false;
				for (int i = 0; i < 200; i++) {
					NPC currentNPC = Main.npc[i];
					if (currentNPC.CanBeChasedBy(this)) {
						Vector2 currentPos = currentNPC.Center;
						float num21 = Math.Abs(projectile.Center.X - currentPos.X) + Math.Abs(projectile.Center.Y - currentPos.Y);
						if (num21 < targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, currentNPC.position, currentNPC.width, currentNPC.height)) {
							targetWeight = num21;
							targetPos = currentPos;
							foundTarget = true;
						}
					}
				}

				if (foundTarget) {

					float scaleFactor = 16f;

					Vector2 targetVelocity = (targetPos - projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
					projectile.velocity = Vector2.Lerp(projectile.velocity, targetVelocity, 0.083333336f);
				}
			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (originPlayer.novaSet && Origins.CanGainHoming[projectile.type]) {
				isHoming = true;
			}
		}
		public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (IsExploding(projectile) && originPlayer.explosiveBlastRadius != StatModifier.Default) {
				StatModifier modifier = originPlayer.explosiveBlastRadius.Scale(additive: 0.5f, multiplicative: 0.5f);
				hitbox.Inflate((int)(modifier.ApplyTo(hitbox.Width) - hitbox.Width), (int)(modifier.ApplyTo(hitbox.Height) - hitbox.Height));
			}
			switch (projectile.type) {
				case ProjectileID.Bomb:
				case ProjectileID.StickyBomb:
				case ProjectileID.Dynamite:
				case ProjectileID.StickyDynamite:
				case ProjectileID.BombFish:
				case ProjectileID.DryBomb:
				case ProjectileID.WetBomb:
				case ProjectileID.LavaBomb:
				case ProjectileID.HoneyBomb:
				case ProjectileID.ScarabBomb:
				if (hitbox.Width < 32) {
					hitbox = default;
				}
				break;
			}
		}
		public static bool IsExploding(Projectile projectile) {
			if (projectile.ModProjectile is IIsExplodingProjectile explodingProjectile) {
				return explodingProjectile.IsExploding();
			}
			switch (projectile.type) {
				default:
				return projectile.timeLeft <= 3 || projectile.penetrate == 0;
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (originPlayer.madHand) {
				target.AddBuff(BuffID.Oiled, 600);
				target.AddBuff(BuffID.OnFire, 600);
			}
		}
		public static int GetVanillaExplosiveType(Projectile projectile) {
			switch (projectile.type) {
				case ProjectileID.Grenade:
				case ProjectileID.BouncyGrenade:
				case ProjectileID.StickyGrenade:
				case ProjectileID.PartyGirlGrenade:
				case ProjectileID.Beenade:
				case ProjectileID.Bomb:
				case ProjectileID.BouncyBomb:
				case ProjectileID.StickyBomb:
				case ProjectileID.Dynamite:
				case ProjectileID.BouncyDynamite:
				case ProjectileID.StickyDynamite:
				case ProjectileID.BombFish:
				case ProjectileID.MolotovCocktail:
				return 1;

				case ProjectileID.RocketI:
				case ProjectileID.RocketII:
				case ProjectileID.RocketIII:
				case ProjectileID.RocketIV:
				case ProjectileID.MiniNukeRocketI:
				case ProjectileID.MiniNukeRocketII:
				case ProjectileID.ClusterRocketI:
				case ProjectileID.ClusterRocketII:
				case ProjectileID.DryRocket:
				case ProjectileID.WetRocket:
				case ProjectileID.LavaRocket:
				case ProjectileID.HoneyRocket:

				case ProjectileID.ProximityMineI:
				case ProjectileID.ProximityMineII:
				case ProjectileID.ProximityMineIII:
				case ProjectileID.ProximityMineIV:
				case ProjectileID.MiniNukeMineI:
				case ProjectileID.MiniNukeMineII:
				case ProjectileID.ClusterMineI:
				case ProjectileID.ClusterMineII:
				case ProjectileID.DryMine:
				case ProjectileID.WetMine:
				case ProjectileID.LavaMine:
				case ProjectileID.HoneyMine:

				case ProjectileID.GrenadeI:
				case ProjectileID.GrenadeII:
				case ProjectileID.GrenadeIII:
				case ProjectileID.GrenadeIV:
				case ProjectileID.MiniNukeGrenadeI:
				case ProjectileID.MiniNukeGrenadeII:
				case ProjectileID.ClusterGrenadeI:
				case ProjectileID.ClusterGrenadeII:
				case ProjectileID.DryGrenade:
				case ProjectileID.WetGrenade:
				case ProjectileID.LavaGrenade:
				case ProjectileID.HoneyGrenade:

				case ProjectileID.RocketSnowmanI:
				case ProjectileID.RocketSnowmanII:
				case ProjectileID.RocketSnowmanIII:
				case ProjectileID.RocketSnowmanIV:
				case ProjectileID.MiniNukeSnowmanRocketI:
				case ProjectileID.MiniNukeSnowmanRocketII:
				case ProjectileID.ClusterSnowmanRocketI:
				case ProjectileID.ClusterSnowmanRocketII:
				case ProjectileID.DrySnowmanRocket:
				case ProjectileID.WetSnowmanRocket:
				case ProjectileID.LavaSnowmanRocket:
				case ProjectileID.HoneySnowmanRocket:

				case ProjectileID.RocketFireworkBlue:
				case ProjectileID.RocketFireworkGreen:
				case ProjectileID.RocketFireworkRed:
				case ProjectileID.RocketFireworkYellow:

				case ProjectileID.Celeb2Rocket:
				case ProjectileID.Celeb2RocketExplosive:
				case ProjectileID.Celeb2RocketLarge:
				case ProjectileID.Celeb2RocketExplosiveLarge:

				case ProjectileID.ElectrosphereMissile:

				case ProjectileID.ClusterFragmentsI:
				case ProjectileID.ClusterFragmentsII:
				case ProjectileID.ClusterSnowmanFragmentsI:
				case ProjectileID.ClusterSnowmanFragmentsII:
				case ProjectileID.HellfireArrow:
				case ProjectileID.Stynger:
				case ProjectileID.StyngerShrapnel:
				case ProjectileID.JackOLantern:
				return 2;

				default:
				return 0;
			}
		}
	}
}
