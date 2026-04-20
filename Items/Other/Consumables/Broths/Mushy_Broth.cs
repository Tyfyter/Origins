using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Mushy_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(6, 157, 44),
				new Color(6, 120, 35),
				new Color(4, 52, 23)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			proj.SpawnProjectile(
				proj.GetSource_OnHit(target),
				proj.Center.Clamp(target.Hitbox),
				default,
				ModContent.ProjectileType<Mushy_Broth_Explosion>(),
				damageDone,
				4
			);
		}
	}
	public class Mushy_Broth_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.ExplosiveVersion[DamageClass.Summon];
		public override int Size => 80;
		public override SoundStyle? Sound => SoundID.Item14.WithVolume(0.66f);
		public override int FireDustAmount => 0;
		public override int SmokeDustAmount => 15;
		public override int SmokeGoreAmount => 2;
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_OnHit { Attacker: Projectile parent, Victim: NPC target }) {
				Projectile.localNPCImmunity[target.whoAmI] = Projectile.localNPCHitCooldown;
				if (MinionGlobalProjectile.IsArtifact(parent)) {
					Projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().projectileBlastRadius += 0.5f;
				} else {
					Projectile.damage -= Projectile.damage / 4;
				}
			}
		}
	}
}