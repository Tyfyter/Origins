using Origins;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Backgrounds {
	public class Void_Background : ModUndergroundBackgroundStyle {
		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Void_Background");
			textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Void_Background");
			textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Void_Background");
			textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Void_Background");
		}
	}
}