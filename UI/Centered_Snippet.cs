using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Centered_Snippet_Handler : ITagHandler {
		internal static Vector2 origin;
		public class Centered_Snippet : WrappingTextSnippet {
			public Centered_Snippet(string text, Color color) {
				this.Text = text;
				this.Color = color;
			}
			public bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, float maxWidth, Vector2 position = default, Color color = default, float scale = 1) {
				Vector2 textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, Text, new(scale), -1);
				size = textSize with {
					X = maxWidth
				};
				if (maxWidth < 0) return false;
				if (justCheckingString || spriteBatch is null) return true;
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, BasePosition + Vector2.UnitX * maxWidth * 0.5f * scale, color, 0, origin + Vector2.UnitX * textSize.X * 0.5f * scale, new(scale));
				return true;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				return UniqueDraw(justCheckingString, out size, spriteBatch, MaxWidth, position, color, scale);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			return new Centered_Snippet(text, baseColor);
		}
	}
}
