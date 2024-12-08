using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Terraria;
using Terraria.GameContent;
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
			return new Evil_Snippet(text, baseColor);
		}
	}
}
