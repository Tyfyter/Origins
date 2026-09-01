using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles; 
//separate global for organization
public class FlailGlobalProjectile : GlobalProjectile {
	public override bool InstancePerEntity => true;
	protected override bool CloneNewInstances => false;
	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.aiStyle == ProjAIStyleID.Flail;
	public override void FlailStats(Projectile projectile, ref int launchTimeLimit, ref float launchSpeed, ref float maxLaunchLength, ref float retractAcceleration, ref float maxRetractSpeed, ref float forcedRetractAcceleration, ref float maxForcedRetractSpeed, ref int ricochetTimeLimit, ref float spinVisualDistance) {
		if (Main.player[projectile.owner].OriginPlayer().automatedReturnsHandler) {
			retractAcceleration *= 2f;
			maxRetractSpeed *= 2f;
			forcedRetractAcceleration *= 2f;
			maxForcedRetractSpeed *= 2f;
		}
	}
}
