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
			switch (entity.type) {
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
				case ProjectileID.RocketI:
				case ProjectileID.RocketII:
				case ProjectileID.RocketIII:
				case ProjectileID.RocketIV:
				case ProjectileID.ProximityMineI:
				case ProjectileID.ProximityMineII:
				case ProjectileID.ProximityMineIII:
				case ProjectileID.ProximityMineIV:
				case ProjectileID.GrenadeI:
				case ProjectileID.GrenadeII:
				case ProjectileID.GrenadeIII:
				case ProjectileID.GrenadeIV:
				case ProjectileID.HellfireArrow:
				return true;
				default:
				return entity.DamageType.CountsAsClass(DamageClasses.Explosive);
			}
		}
		public override void SetDefaults(Projectile projectile) {
			isHoming = false;
		}
		public override void AI(Projectile projectile) {
			if (isHoming) {

			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			
		}
		public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (IsExploding(projectile) && originPlayer.explosiveBlastRadius != StatModifier.Default) {
				StatModifier modifier = originPlayer.explosiveBlastRadius.Scale(scale: 0.5f);
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
			return projectile.timeLeft <= 3 || projectile.penetrate == 0;
		}
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (originPlayer.madHand) {
				target.AddBuff(BuffID.Oiled, 600);
				target.AddBuff(BuffID.OnFire, 600);
			}
		}
	}
}
