using Origins;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Backgrounds {
	public class VoidBG : ModUgBgStyle {
		public override bool ChooseBgStyle() {
			return Main.LocalPlayer.GetModPlayer<OriginPlayer>().ZoneVoid;
		}

		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = mod.GetBackgroundSlot("Backgrounds/Void_Test");
			textureSlots[1] = mod.GetBackgroundSlot("Backgrounds/Void_Test");
			textureSlots[2] = mod.GetBackgroundSlot("Backgrounds/Void_Test");
			textureSlots[3] = mod.GetBackgroundSlot("Backgrounds/Void_Test");
		}
	}
}