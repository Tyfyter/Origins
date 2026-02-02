using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Metallic_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0xFF0000),
				FromHexRGB(0x980000),
				FromHexRGB(0x510000)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Ichor, 300);
			if (MinionGlobalProjectile.IsArtifact(proj)) {
				if (target.HasBuff(BuffID.Bleeding)) {
					OriginExtensions.MinionLifeSteal(proj, target, damageDone, 0.7f);
				}
				target.AddBuff(BuffID.Bleeding, 300);
			}
		}
	}
}