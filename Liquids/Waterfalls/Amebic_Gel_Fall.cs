using ModLiquidLib.ModLoader;
using Terraria;

namespace Origins.Liquids.Waterfalls {
	public class Amebic_Gel_Fall : ModLiquidFall {
		public override bool PlayWaterfallSounds() {
			return true;
		}
		public override void AddLight(int i, int j) {
			float mult = 0.666f * World.BiomeData.Riven_Hive.NormalGlowValue.GetValue();
			Lighting.AddLight(i, j, 0, 0.9f * mult, 1f * mult);
		}
	}
}
