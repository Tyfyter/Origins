using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
    public class Ashen_Water_Style : ModWaterStyle {
		public override int ChooseWaterfallStyle() => ModContent.GetInstance<Ashen_Waterfall_Style>().Slot;
		public override int GetDropletGore() => GoreID.PearlsandDrip;
		public override int GetSplashDust() => DustID.Mud;
		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			(r, g, b) = Color.Black.ToVector3();
		}
		public override Color BiomeHairColor() => Color.OrangeRed;
	}
	public class Ashen_Waterfall_Style : ModWaterfallStyle {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Slot;
		}
		public override void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
			Vector3 mult = Color.Black.ToVector3() / 10;
			r *= mult.X;
			g *= mult.Y;
			b *= mult.Z;
		}
    }
}
