using Terraria;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Header_Snippet_Handler : ITagHandler {
		public class Header_Snippet(string text, Color color = default) : TextSnippet(text.Replace(" ", "  "), color, 1.1f) { }
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			return new Header_Snippet(text, baseColor);
		}
	}
}
