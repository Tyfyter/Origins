using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Sweet_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(172, 0, 6),
				new(189, 87, 124),
				new(255, 75, 249)
			];
		}
		public override void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (Origins.ArtifactMinion[proj.type]) modifiers.SourceDamage *= 1.1f;
		}
		public override void PreUpdateMinion(Projectile minion) {
			minion.GetGlobalProjectile<MinionGlobalProjectile>().tempBonusUpdates += 0.15f;
		}
	}
}
