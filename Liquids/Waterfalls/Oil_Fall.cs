using ModLiquidLib.ModLoader;
using Terraria;
using Terraria.ID;

namespace Origins.Liquids.Waterfalls {
	//An example of the ModLiquidFall class (although pretty empty here, a proper example will be made soon)
	public class Oil_Fall : ModLiquidFall {
		public override string Texture => base.Texture.Replace("Burning_", string.Empty);
		public override void SetStaticDefaults() {
			WaterfallDist = 4;
		}
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
			float num = 0.8f; //the strength we usually want
			if (s > maxSteps - 10)
				num *= (maxSteps - s) / 10f; //modifies the strength based on how faded the waterfall is based on length
			return num;
		}
	}
	public class Burning_Oil_Fall : Oil_Fall {
		public override float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache) {
			float? alpha() {
				float num = 0.8f; //the strength we usually want
				if (s > maxSteps - 10)
					num *= (maxSteps - s) / 10f; //modifies the strength based on how faded the waterfall is based on length
				return num;
			}
			float? a = alpha();
			Vector2 pos = new Vector2(x, y) * 16;
			if (a.HasValue && a.Value > 0.2f && Main.rand.NextBool(200)) Dust.NewDust(pos, 1, 1, DustID.Torch);
			return a;
		}
	}
}
