using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using System.Diagnostics;
using Terraria;

namespace Origins.Core; 
internal class IgnoreThisLineOfTheStackTraceThisCodeChangesNothingItJustReadsData {
	internal static void LoadCurrentEntity() {
		On_Projectile.Update += [DebuggerStepThrough] (orig, self, i) => {
			using CurrentEntity cur = new(self);
			orig(self, i);
		};
		On_Player.Update += [DebuggerStepThrough] (orig, self, i) => {
			using CurrentEntity cur = new(self);
			orig(self, i);
		};
		On_NPC.UpdateNPC += [DebuggerStepThrough] (orig, self, i) => {
			using CurrentEntity cur = new(self);
			orig(self, i);
		};
	}
	internal static void LoadCurrentEntityShimmer() {
		On_Projectile.Update += [DebuggerStepThrough] (orig, self, i) => {
			using (Weak_Shimmer_Debuff.isUpdatingShimmeryThing.ScopedOverride(self.TryGetGlobalProjectile(out OriginGlobalProj proj) && proj.weakShimmer)) {
				orig(self, i);
			}
		};
	}
}
