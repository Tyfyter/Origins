using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Astringent_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0x4E2259),
				FromHexRGB(0x703680),
				FromHexRGB(0x72457E)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			bool artiMinion = MinionGlobalProjectile.IsArtifact(proj);
			target.AddBuff(BuffID.CursedInferno, artiMinion ? 300 : 120);
			if (artiMinion) target.AddBuff(BuffID.ShadowFlame, 180);
		}
	}
}