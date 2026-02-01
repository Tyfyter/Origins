using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	//TODO: remove, moved to PegasusLib
	public class CustomExpertScaling : ILoadable {
		public void Load(Mod mod) {
			On_NPC.ScaleStats_ApplyExpertTweaks += (orig, self) => {
				orig(self);
				OriginsSets.NPCs.CustomExpertScaling.GetIfInRange(self.type)?.Invoke(self);
			};
		}
		public void Unload() { }
	}
}
