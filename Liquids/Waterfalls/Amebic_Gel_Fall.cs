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
		public override void AnimateWaterfall(ref int frame, ref int frameBackground, ref int frameCounter) {
			if (frameCounter.CycleUp(5)) frame.CycleUp(15);
		}
	}
}
