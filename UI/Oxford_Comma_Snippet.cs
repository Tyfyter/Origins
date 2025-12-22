using Terraria;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Oxford_Comma_Handler : ITagHandler {
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			return new Oxford_Comma_Snippet(text, baseColor);
		}
	}
	public class Not_Oxford_Comma_Handler : ITagHandler {
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			return new Oxford_Comma_Snippet(text, baseColor, true);
		}
	}
#pragma warning disable CS9107
	public class Oxford_Comma_Snippet(string text, Color baseColor, bool inverted = false) : TextSnippet(text, baseColor) {
#pragma warning restore CS9107
		public override void Update() {
			if (OriginClientConfig.Instance.OxfordComma == inverted) {
				Text = "";
			} else {
				Text = text;
			}
		}
	}
}
