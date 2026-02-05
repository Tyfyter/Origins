using ModLiquidLib.ModLoader;
using Terraria;

namespace Origins.Liquids.Waterfalls {
	//An example of the ModLiquidFall class (although pretty empty here, a proper example will be made soon)
	public class OilFall : ModLiquidFall {
		//Removes the waterfall sound that waterfalls normally make.
		//useful for when the waterfall is not ment to make waterfall sounds
		public override bool PlayWaterfallSounds() {
			return false;
		}

		//Usually waterfalls draw as a slight opacity
		//Lava, Honey and shimmer all draw at a slight higher opacity than water
		//We can modify how strong the alpha is.
		//0 (un-see-able), 1 (fully opaque)
		public override float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache) {
			float num = 1f; //the strength we usually want
			if (s > maxSteps - 20)
				num *= (maxSteps - s) / 10f; //modifies the strength based on how faded the waterfall is based on length
			return num;
		}
	}
}
