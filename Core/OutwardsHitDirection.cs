using Terraria;
using Terraria.ModLoader;

namespace Origins.Core; 
public static class OutwardsHitDirection {
	public static ref bool UseOutwardsHitDirection(this Projectile projectile) => ref projectile.GetGlobalProjectile<OutwardsHitDirectionGlobal>().active;
	class OutwardsHitDirectionGlobal : GlobalProjectile {
		public bool active = false;
		public override bool InstancePerEntity => true;
		public override bool? CanHitNPC(Projectile projectile, NPC target) {
			if (active) projectile.direction = (projectile.Center.X < target.Center.X).ToDirectionInt();
			return base.CanHitNPC(projectile, target);
		}
	}
}
