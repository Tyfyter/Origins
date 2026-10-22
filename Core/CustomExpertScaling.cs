using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	public class CustomExpertScaling : ILoadable {
		struct Moved : IMovedToPegFlag;
		public void Load(Mod mod) {
			On_NPC.ScaleStats_ApplyExpertTweaks += (orig, self) => {
				orig(self);
				OriginsSets.NPCs.CustomExpertScaling.GetIfInRange(self.type)?.Invoke(self);
			};
		}
		public void Unload() { }
	}
}
