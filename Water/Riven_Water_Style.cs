using Microsoft.Xna.Framework;
using Origins.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
    public class Riven_Water_Style : ModWaterStyle, IGlowingWaterStyle {
		public override int ChooseWaterfallStyle() => ModContent.GetInstance<Riven_Waterfall_Style>().Slot;
		public override int GetDropletGore() => GoreID.ChimneySmoke1 + Main.rand.Next(3);
		public override int GetSplashDust() => 99;
		public static float GlowValue => World.BiomeData.Riven_Hive.NormalGlowValue.GetValue() * 0.5f + 0.25f;
		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			float glowValue = GlowValue;
			r = 0.1f * glowValue;
			g = 1.05f * glowValue;
			b = 1f * glowValue;
		}
		public override Color BiomeHairColor() => new(50, 250, 230);
		public void AddLight(ref Vector3 color, byte liquidAmount) {
			float mult = (liquidAmount > 200 ? 1 : liquidAmount / 200) * World.BiomeData.Riven_Hive.NormalGlowValue.GetValue();

			color.Y += 0.9f * mult;
			color.Z += 1f * mult;
		}
	}
	public class Riven_Waterfall_Style : ModWaterfallStyle {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Slot;
		}
		public override void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
			float _blueWave = 1f;
			if (LightingMethods._activeEngine.GetValue() is LegacyLighting legacyLighting) _blueWave = LightingMethods._blueWave.GetValue(legacyLighting);
			float glowValue = Riven_Water_Style.GlowValue
				* 0.91f
				* _blueWave;
			//float e = 1 - glowValue;
			//glowValue = glowValue * e + (1 - e);
			r = 0.1f * glowValue * 255f;
			g = 1.05f * glowValue * 255f;
			b = 1f * glowValue * 255f;
			//r *= 0.1f * glowValue;
			//g *= 1.05f * glowValue;
			//b *= 1f * glowValue;
		}
		public override void AddLight(int i, int j) {
			float mult = 0.666f * World.BiomeData.Riven_Hive.NormalGlowValue.GetValue();
			Lighting.AddLight(i, j, 0, 0.9f * mult, 1f * mult);
		}
    }
}
