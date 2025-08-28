using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace Origins.UI {
	public class Glitch_Handler : ITagHandler {
		internal static Vector2 origin;
		public class Glitch_Snippet(string text, string symbols, Color color = default, float scale = 1) : TextSnippet(text, color, scale) {
			readonly StringBuilder DisplayedText = new(text);
			public override void Update() {
				UnifiedRandom rand = new((int)(Origins.gameFrameCount / 32));
				rand = new((int)(Origins.gameFrameCount >> (2 + rand.Next(2))));
				for (int i = rand.Next(Text.Length / 4); i > 0; i--) {
					DisplayedText[rand.Next(DisplayedText.Length)] = symbols[rand.Next(symbols.Length)];
				}
				Text = DisplayedText.ToString();
			}
		}
		public record struct Options(float Speed = 1f / 60f, float WiggleWidth = 16, float WiggleScale = 2);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			return new Glitch_Snippet(text, options, baseColor, 1);
		}
	}
}
