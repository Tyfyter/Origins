using Microsoft.Xna.Framework;
using Origins.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
    public class Defiled_Water_Style : ModWaterStyle {
		public override int ChooseWaterfallStyle() => ModContent.GetInstance<Defiled_Waterfall_Style>().Slot;
		public override int GetDropletGore() => GoreID.PearlsandDrip;
		public override int GetSplashDust() => DustID.RainCloud;
		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = g = b = 0.9475f;
		}
		public override Color BiomeHairColor() => new(150, 150, 150);
	}
	public class Defiled_Waterfall_Style : ModWaterfallStyle {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Slot;
		}
		public override void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
			r *= 0.9475f;
			g *= 0.9475f;
			b *= 0.9475f;
		}
    }
}
