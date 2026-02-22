using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
    public class Riven_Water_Style : ModWaterStyle {
		public override int ChooseWaterfallStyle() => ModContent.GetInstance<Riven_Waterfall_Style>().Slot;
		public override int GetDropletGore() => GoreID.ChimneySmoke1 + Main.rand.Next(3);
		public override int GetSplashDust() => 99;
		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			float glowValue = 1 / 1.05f;
			r = 0.1f * glowValue;
			g = 1.05f * glowValue;
			b = 1f * glowValue;
		}
		public override Color BiomeHairColor() => new(50, 250, 230);
	}
	public class Riven_Waterfall_Style : ModWaterfallStyle {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Slot;
		}
    }
}
