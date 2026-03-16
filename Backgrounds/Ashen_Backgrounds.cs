using Terraria;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	public class Ashen_Surface_Background : ModSurfaceBackgroundStyle {
		public override int ChooseFarTexture() {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background3");
		}

		public override int ChooseMiddleTexture() {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background2");
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background");
		}

		public override void ModifyFarFades(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) MathUtils.LinearSmoothing(ref fades[i], (i == Slot).ToInt(), transitionSpeed);
		}
	}
	public class Ashen_Underground_Background : ModUndergroundBackgroundStyle {
		public override void FillTextureArray(int[] textureSlots) {
			for (int i = 0; i < 4; i++) {
				textureSlots[i] = ModContent.GetModBackgroundSlot(GetType().GetDefaultTMLName() + i);
			}
			textureSlots[4] = 128 + Main.hellBackStyle;
		}
	}
}