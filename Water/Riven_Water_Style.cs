using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
    public class Riven_Water_Style : ModWaterStyle, IGlowingWaterStyle {
		public override int ChooseWaterfallStyle() => ModContent.GetInstance<Riven_Waterfall_Style>().Slot;
		public override int GetDropletGore() => GoreID.ChimneySmoke1 + Main.rand.Next(3);
		public override int GetSplashDust() => 99;
		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			float glowValue = World.BiomeData.Riven_Hive.NormalGlowValue.GetValue() * 0.5f + 0.25f;
			r = 0.1f * glowValue;
			g = 1.05f * glowValue;
			b = 1f * glowValue;
		}
		public override Color BiomeHairColor() => new Color(50, 250, 230);

		public void AddLight(ref Vector3 color, byte liquidAmount) {
			float mult = (liquidAmount > 200 ? 1 : liquidAmount / 200) * World.BiomeData.Riven_Hive.NormalGlowValue.GetValue();

			color.Y += 0.9f * mult;
			color.Z += 1f * mult;
		}
	}
	public class Riven_Waterfall_Style : ModWaterfallStyle {
		public override void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
			r = 0.1f;
			g = 1.05f;
			b = 1f;
		}
	}
}
