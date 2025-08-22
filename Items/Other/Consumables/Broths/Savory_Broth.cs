using Origins.Buffs;
using Origins.NPCs;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Savory_Broth : BrothBase, ITornSource {
		public float Severity => 0.05f;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(235, 147, 13),
				new(181, 109, 0),
				new(141, 85, 0)
			];
		}
		public override int Duration => 6;
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (MinionGlobalProjectile.IsArtifact(proj) && proj.TryGetOwner(out Player player)) {
				OriginGlobalNPC.InflictTorn(target, 90, 180, Severity, player.OriginPlayer());
			}
		}
	}
}
