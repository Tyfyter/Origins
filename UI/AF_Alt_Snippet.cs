using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class AF_Alt_Handler : ITagHandler {
		public class AF_Alt_Snippet : WrappingTextSnippet {
			public AF_Alt_Snippet(string text, Color color) {
				Text = text;
				Color = color;
			}
			public bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, float maxWidth, Vector2 position = default, Color color = default, float scale = 1) {
				if (color == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
					color = Color.White;
				} else if (color.A == Main.mouseTextColor) {
					color *= 255f / Main.mouseTextColor;
				}
				TextSnippet[] snippets = ChatManager.ParseMessage(Text, color).ToArray();
				size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, new(scale), MaxWidth);
				if (justCheckingString || spriteBatch is null) return true;

				TextSnippet[] _snippets = new TextSnippet[snippets.Length + 1];
				float padding = position.X - BasePosition.X;
				_snippets[0] = new PaddingSnippet(padding);
				snippets.CopyTo(_snippets, 1);
				position.X = BasePosition.X;
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, _snippets, position, color, 0, Vector2.Zero, new(scale), out int hoveredSnippet, maxWidth);
				if (hoveredSnippet >= 0 && hoveredSnippet < _snippets.Length && _snippets[hoveredSnippet].CheckForHover) {
					_snippets[hoveredSnippet].OnHover();
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						_snippets[hoveredSnippet].OnClick();
					}
				}
				return true;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				return UniqueDraw(justCheckingString, out size, spriteBatch, MaxWidth, position, color, scale);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			string[] parts = text.Split(';');
			string normalKey = $"Mods.Origins.{parts[0]}";
			string afKey = $"Mods.Origins.AprilFools.{parts[0]}";
			string key = OriginsModIntegrations.CheckAprilFools() && Language.Exists(afKey) ? afKey : normalKey;
			return new AF_Alt_Snippet(Language.GetTextValue(key, parts[1..]), baseColor);
		}
	}
}
