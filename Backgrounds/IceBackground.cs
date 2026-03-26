using Terraria;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	public abstract class IceBackground : ModUndergroundBackgroundStyle {
		int? background;
		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = 40;
			textureSlots[1] = 33;
			textureSlots[2] = 34;
			textureSlots[4] = 128 + Main.hellBackStyle;
			textureSlots[3] = background ??= ModContent.GetModBackgroundSlot(GetType().GetDefaultTMLName());
		}
	}
}