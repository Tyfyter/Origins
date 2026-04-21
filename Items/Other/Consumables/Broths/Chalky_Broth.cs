using Origins.Buffs;
using Origins.NPCs;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Chalky_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(246, 240, 207),
				new Color(221, 214, 170),
				new Color(181, 174, 130)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) => OriginGlobalNPC.InflictImpedingShrapnel(target, 180);
		public override void OnHurt(Projectile minion, int damage, bool fromDoT, IArtifactDamageSource damageSource) {
			for (int i = fromDoT ? 1 : 3; i > 0; i--) {
				minion.SpawnProjectile(
					minion.GetSource_Death(),
					minion.Center,
					Main.rand.NextVector2CircularEdge(4, 4),
					Impeding_Shrapnel_Shard.ID,
					40,
					2
				);
			}
		}
		public override void OnKill(Projectile minion) {
			if (!MinionGlobalProjectile.IsArtifact(minion)) return;
			for (int i = 6; i > 0; i--) {
				minion.SpawnProjectile(
					minion.GetSource_Death(),
					minion.Center,
					Main.rand.NextVector2CircularEdge(4, 4),
					Impeding_Shrapnel_Shard.ID,
					40,
					2
				);
			}
		}
	}
}