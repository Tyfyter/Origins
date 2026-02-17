using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
	public class Brine_Water_Style : ModWaterStyle {
		public override int ChooseWaterfallStyle() => ModContent.GetInstance<Brine_Waterfall_Style>().Slot;
		public override int GetDropletGore() => GoreID.ChimneySmoke1 + Main.rand.Next(3);
		public override int GetSplashDust() => DustID.Water_Jungle;
		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = 0.5f;
			g = 0.9f;
			b = 0.83f;
		}
		public override Color BiomeHairColor() => new(20, 102, 87);
	}
	public class Brine_Waterfall_Style : ModWaterfallStyle {
		public override void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
			r *= 0.5f;
			g *= 0.9f;
			b *= 0.83f;
		}
	}
}
