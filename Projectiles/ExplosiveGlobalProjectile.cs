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
		bool magicTripwire = false;
		bool magicTripwireTripped = false;
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
			if (magicTripwire && Origins.MagicTripwireRange[projectile.type] > 0) {
				int magicTripwireRange = Origins.MagicTripwireRange[projectile.type];
				Rectangle magicTripwireHitbox = new Rectangle(
					(int)projectile.Center.X - magicTripwireRange,
					(int)projectile.Center.Y - magicTripwireRange,
					magicTripwireRange * 2,
					magicTripwireRange * 2
				);
				bool tripped = false;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy() && magicTripwireHitbox.Intersects(npc.Hitbox)) {
						tripped = true;
					}
				}
				if (tripped) {
					magicTripwireTripped = true;
				} else if (magicTripwireTripped) {
					Explode(projectile);
				}
			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (originPlayer.novaSet && Origins.CanGainHoming[projectile.type]) {
				isHoming = true;
			}
			if (originPlayer.iwtpaStandard) {
				projectile.timeLeft -= projectile.timeLeft / 3;
			}
			if (originPlayer.magicTripwire) {
				magicTripwire = true;
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
		public static void Explode(Projectile projectile, int delay = 0) {
			if (projectile.ModProjectile is IIsExplodingProjectile explodingProjectile) {
				explodingProjectile.Explode(delay);
			} else {
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
					case ProjectileID.DryBomb:
					case ProjectileID.WetBomb:
					case ProjectileID.LavaBomb:
					case ProjectileID.HoneyBomb:
					case ProjectileID.ScarabBomb:
					case ProjectileID.MolotovCocktail:
					delay += 3;
					goto default;

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
					projectile.velocity = Vector2.Zero;
					delay += 3;
					goto default;

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
					default:
					if (projectile.timeLeft > delay) projectile.timeLeft = delay;
					break;
				}
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
				case ProjectileID.DryBomb:
				case ProjectileID.WetBomb:
				case ProjectileID.LavaBomb:
				case ProjectileID.HoneyBomb:
				case ProjectileID.ScarabBomb:
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
		internal static void SetupMagicTripwireRanges(int[] magicTripwireRange) {
			magicTripwireRange[ProjectileID.Grenade] = 32;
			magicTripwireRange[ProjectileID.BouncyGrenade] = 32;
			magicTripwireRange[ProjectileID.StickyGrenade] = 32;
			magicTripwireRange[ProjectileID.PartyGirlGrenade] = 32;
			magicTripwireRange[ProjectileID.Beenade] = 32;
			magicTripwireRange[ProjectileID.Bomb] = 64;
			magicTripwireRange[ProjectileID.BouncyBomb] = 64;
			magicTripwireRange[ProjectileID.StickyBomb] = 64;
			magicTripwireRange[ProjectileID.Dynamite] = 64;
			magicTripwireRange[ProjectileID.BouncyDynamite] = 64;
			magicTripwireRange[ProjectileID.StickyDynamite] = 64;
			magicTripwireRange[ProjectileID.BombFish] = 64;
			magicTripwireRange[ProjectileID.DryBomb] = 12;
			magicTripwireRange[ProjectileID.WetBomb] = 12;
			magicTripwireRange[ProjectileID.LavaBomb] = 12;
			magicTripwireRange[ProjectileID.HoneyBomb] = 12;
			magicTripwireRange[ProjectileID.ScarabBomb] = 0;
			magicTripwireRange[ProjectileID.MolotovCocktail] = 64;

			magicTripwireRange[ProjectileID.RocketI] = 32;
			magicTripwireRange[ProjectileID.RocketII] = 32;
			magicTripwireRange[ProjectileID.RocketIII] = 64;
			magicTripwireRange[ProjectileID.RocketIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeRocketI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeRocketII] = 64;
			magicTripwireRange[ProjectileID.ClusterRocketI] = 32;
			magicTripwireRange[ProjectileID.ClusterRocketII] = 32;
			magicTripwireRange[ProjectileID.DryRocket] = 32;
			magicTripwireRange[ProjectileID.WetRocket] = 32;
			magicTripwireRange[ProjectileID.LavaRocket] = 32;
			magicTripwireRange[ProjectileID.HoneyRocket] = 32;

			magicTripwireRange[ProjectileID.ProximityMineI] = 32;
			magicTripwireRange[ProjectileID.ProximityMineII] = 32;
			magicTripwireRange[ProjectileID.ProximityMineIII] = 64;
			magicTripwireRange[ProjectileID.ProximityMineIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeMineI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeMineII] = 64;
			magicTripwireRange[ProjectileID.ClusterMineI] = 32;
			magicTripwireRange[ProjectileID.ClusterMineII] = 32;
			magicTripwireRange[ProjectileID.DryMine] = 32;
			magicTripwireRange[ProjectileID.WetMine] = 32;
			magicTripwireRange[ProjectileID.LavaMine] = 32;
			magicTripwireRange[ProjectileID.HoneyMine] = 32;

			magicTripwireRange[ProjectileID.GrenadeI] = 32;
			magicTripwireRange[ProjectileID.GrenadeII] = 32;
			magicTripwireRange[ProjectileID.GrenadeIII] = 64;
			magicTripwireRange[ProjectileID.GrenadeIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeGrenadeI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeGrenadeII] = 64;
			magicTripwireRange[ProjectileID.ClusterGrenadeI] = 32;
			magicTripwireRange[ProjectileID.ClusterGrenadeII] = 32;
			magicTripwireRange[ProjectileID.DryGrenade] = 32;
			magicTripwireRange[ProjectileID.WetGrenade] = 32;
			magicTripwireRange[ProjectileID.LavaGrenade] = 32;
			magicTripwireRange[ProjectileID.HoneyGrenade] = 32;

			magicTripwireRange[ProjectileID.RocketSnowmanI] = 32;
			magicTripwireRange[ProjectileID.RocketSnowmanII] = 32;
			magicTripwireRange[ProjectileID.RocketSnowmanIII] = 64;
			magicTripwireRange[ProjectileID.RocketSnowmanIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeSnowmanRocketI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeSnowmanRocketII] = 64;
			magicTripwireRange[ProjectileID.ClusterSnowmanRocketI] = 32;
			magicTripwireRange[ProjectileID.ClusterSnowmanRocketII] = 32;
			magicTripwireRange[ProjectileID.DrySnowmanRocket] = 32;
			magicTripwireRange[ProjectileID.WetSnowmanRocket] = 32;
			magicTripwireRange[ProjectileID.LavaSnowmanRocket] = 32;
			magicTripwireRange[ProjectileID.HoneySnowmanRocket] = 32;

			magicTripwireRange[ProjectileID.RocketFireworkBlue] = 64;
			magicTripwireRange[ProjectileID.RocketFireworkGreen] = 64;
			magicTripwireRange[ProjectileID.RocketFireworkRed] = 64;
			magicTripwireRange[ProjectileID.RocketFireworkYellow] = 64;

			magicTripwireRange[ProjectileID.Celeb2Rocket] = 64;
			magicTripwireRange[ProjectileID.Celeb2RocketExplosive] = 64;
			magicTripwireRange[ProjectileID.Celeb2RocketLarge] = 64;
			magicTripwireRange[ProjectileID.Celeb2RocketExplosiveLarge] = 64;

			magicTripwireRange[ProjectileID.ElectrosphereMissile] = 32;

			magicTripwireRange[ProjectileID.HellfireArrow] = 8;
			magicTripwireRange[ProjectileID.Stynger] = 12;
			magicTripwireRange[ProjectileID.JackOLantern] = 32;
		}
	}
}
