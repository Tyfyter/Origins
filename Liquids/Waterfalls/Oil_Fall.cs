using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ModLoader;
using Terraria;

namespace Origins.Liquids.Waterfalls {
	//An example of the ModLiquidFall class (although pretty empty here, a proper example will be made soon)
	public class Oil_Fall : ModLiquidFall {
		//Removes the waterfall sound that waterfalls normally make.
		//useful for when the waterfall is not ment to make waterfall sounds
		public override bool PlayWaterfallSounds() {
			return false;
		}
		public override bool PreDraw(WaterfallManager.WaterfallData currentWaterfallData, int i, int j, SpriteBatch spriteBatch) {
			WaterfallDist = 25;
			return true;
		}
		//Usually waterfalls draw as a slight opacity
		//Lava, Honey and shimmer all draw at a slight higher opacity than water
		//We can modify how strong the alpha is.
		//0 (un-see-able), 1 (fully opaque)
		public override float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache) {
			float num = 0.8f; //the strength we usually want
			if (s > maxSteps - 5)
				num *= (maxSteps - s) / 5f; //modifies the strength based on how faded the waterfall is based on length
			return num;
		}
	}
	public class Burning_Oil_Fall : Oil_Fall { }
}
