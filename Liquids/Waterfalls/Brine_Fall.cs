using ModLiquidLib.ModLoader;
using Terraria;

namespace Origins.Liquids.Waterfalls {
	public class Brine_Fall : ModLiquidFall {
		public override bool PlayWaterfallSounds() {
			return true;
		}
		public override float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache) {
			return null;
			float num = 0.8f; //the strength we usually want
			if (s > maxSteps - 10)
				num *= (maxSteps - s) / 10f; //modifies the strength based on how faded the waterfall is based on length
			return num;
		}
	}
}
