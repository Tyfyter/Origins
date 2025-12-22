using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace Origins.UI {
	public class Glitch_Handler : ITagHandler {
#pragma warning disable CS0649 // Field is never assigned to
		internal static Vector2 origin;
#pragma warning restore CS0649 // Field is never assigned to
		public class Glitch_Snippet(string text, string symbols, Color color = default, float scale = 1) : TextSnippet(text, color, scale) {
			readonly StringBuilder DisplayedText = new(text);
			string OriginalText = text;
			public override void Update() {
				if (string.IsNullOrEmpty(OriginalText)) OriginalText = Lang.GetItemNameValue(Main.rand.Next(ItemLoader.ItemCount));
				if (!string.IsNullOrEmpty(symbols)) DisplayedText.Replace(Text, OriginalText);
				UnifiedRandom rand = new((int)(Origins.gameFrameCount / 32));
				rand = new((int)(Origins.gameFrameCount >> (2 + rand.Next(2))));
				for (int i = rand.Next(Text.Length / 4 + 1); i > 0; i--) {
					int index = rand.Next(DisplayedText.Length);
					if (string.IsNullOrEmpty(symbols)) {
						unchecked {
							DisplayedText[index] = (char)(DisplayedText[index] + rand.Next(-4, 5));
						}
					} else {
						DisplayedText[index] = symbols[rand.Next(symbols.Length)];
					}
				}
				Text = DisplayedText.ToString();
			}
		}
		public record struct Options(float Speed = 1f / 60f, float WiggleWidth = 16, float WiggleScale = 2);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			return new Glitch_Snippet(text, options, baseColor, 1);
		}
	}
}
