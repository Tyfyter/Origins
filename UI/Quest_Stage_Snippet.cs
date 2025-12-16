using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Quest_Stage_Snippet_Handler : ITagHandler {
		public class Quest_Stage_Snippet : WrappingTextSnippet {
			readonly bool completed;
			readonly TextSnippet[] snippets;
			public Quest_Stage_Snippet(string text, Color color, bool completed) {
				Text = text.Replace('<', '[').Replace('>', ']');
				snippets = ChatManager.ParseMessage(Text, color).ToArray();
				this.Color = color;
				this.completed = completed;
			}
			public bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, float maxWidth, Vector2 position = default, Color color = default, float scale = 1) {
				size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, new(scale), -1);
				if (justCheckingString || spriteBatch is null) return true;
				if (completed) color *= 0.666f;

				TextSnippet[] _snippets = new TextSnippet[snippets.Length + 1];
				float padding = position.X - BasePosition.X;
				_snippets[0] = new PaddingSnippet(padding);
				snippets.CopyTo(_snippets, 1);
				position.X = BasePosition.X;
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, _snippets, position, color, 0, Vector2.Zero, new(scale), out int hoveredSnippet, maxWidth, completed);
				if (hoveredSnippet >= 0 && hoveredSnippet < _snippets.Length && _snippets[hoveredSnippet].CheckForHover) {
					_snippets[hoveredSnippet].OnHover();
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						_snippets[hoveredSnippet].OnClick();
					}
				}
				if (completed) {
					ChatManager.DrawColorCodedString(spriteBatch, OriginExtensions.StrikethroughFont, _snippets, position, new Color(color.R, color.G, color.B, 255), 0, Vector2.Zero, new(scale), out _, maxWidth, completed);
					return true;
				}
				return true;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				return UniqueDraw(justCheckingString, out size, spriteBatch, MaxWidth, position, color, scale);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			bool completed = options == "completed";
			return new Quest_Stage_Snippet(text, baseColor, completed);
		}
	}
}
