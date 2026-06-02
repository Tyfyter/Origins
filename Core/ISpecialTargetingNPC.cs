using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	interface ISpecialTargetingNPC {
		void TargetClosest(bool faceTarget = true, Vector2? checkPosition = null);
		struct Loader : IAutoload<Loader>, IAutoloader {
			static void IAutoloader.Autoload(Mod mod, Type type) {
				On_NPC.TargetClosest += On_NPC_TargetClosest;
				On_NPC.TargetClosestUpgraded += On_NPC_TargetClosestUpgraded;
			}
			static void On_NPC_TargetClosest(On_NPC.orig_TargetClosest orig, NPC self, bool faceTarget) {
				if (self.ModNPC is ISpecialTargetingNPC specialTargeting) {
					specialTargeting.TargetClosest(faceTarget, null);
					return;
				}
				orig(self, faceTarget);
			}
			private static void On_NPC_TargetClosestUpgraded(On_NPC.orig_TargetClosestUpgraded orig, NPC self, bool faceTarget, Vector2? checkPosition) {
				if (self.ModNPC is ISpecialTargetingNPC specialTargeting) {
					specialTargeting.TargetClosest(faceTarget, checkPosition);
					return;
				}
				orig(self, faceTarget, checkPosition);
			}
		}
		struct MoveToPegFlag : IMoveToPegFlag;
	}
}
