using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Evil_Handler : ITagHandler {
		public class Evil_Snippet : TextSnippet {
			public Evil_Snippet(string text, Color color = default) : base(text) {
				Color = color;
				ModContent.TryFind(text, out AltBiome biome);
				Text = (biome ?? WorldBiomeManager.GetWorldEvil(true)).DisplayName.Value;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			return new Evil_Snippet(text, baseColor);
		}
	}
}
