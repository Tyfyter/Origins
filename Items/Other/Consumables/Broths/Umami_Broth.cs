using Origins.Buffs;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Umami_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(140, 0, 124),
				new(165, 0, 85),
				new(86, 174, 81)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			bool artifact = Origins.ArtifactMinion[proj.type];
			target.AddBuff(BuffID.Venom, artifact ? 120 : 60);
			if (artifact) target.AddBuff(Toxic_Shock_Debuff.ID, 60);
		}
	}
}
