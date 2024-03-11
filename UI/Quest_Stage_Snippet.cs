using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Text;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Quest_Stage_Snippet_Handler : ITagHandler {
		public class Quest_Stage_Snippet : TextSnippet {
			bool completed;
			public Quest_Stage_Snippet(string text, Color color, bool completed) {
				Text = text;
				this.Color = color;
				this.completed = completed;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1) {
				if (justCheckingString || spriteBatch is null) {
					size = FontAssets.MouseText.Value.MeasureString(Text);
					return true;
				}
				if (completed) {
					StringBuilder builder = new StringBuilder();
					Vector2 dimensions = FontAssets.MouseText.Value.MeasureString(Text);
					DynamicSpriteFont strikethroughFont = OriginExtensions.StrikethroughFont;
					size = dimensions;
					if (justCheckingString) return false;
					const char strike = '–';
					float strikeWidth = strikethroughFont.MeasureString(strike.ToString()).X - 2;
					for (int i = (int)Math.Ceiling(dimensions.X / strikeWidth); i-- > 0;) {
						builder.Append(strike);
					}
					color *= 0.666f;
					ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
					ChatManager.DrawColorCodedString(spriteBatch, strikethroughFont, builder.ToString(), position + dimensions * new Vector2(0.5f, 0.025f), new Color(color.R, color.G, color.B, 255), 0, new Vector2(strikeWidth * builder.Length * 0.5f, 0), new Vector2(scale));
					//ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, builder.ToString(), position + dimensions * new Vector2(0.5f, 0.025f), new Color(color.R, color.G, color.B, 255), 0, new Vector2(strikeWidth * builder.Length * 0.5f, 0), new Vector2(scale));
					return true;
				} else {
					size = FontAssets.MouseText.Value.MeasureString(Text);
					ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
					return true;
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			bool completed = options == "completed";
			return new Quest_Stage_Snippet(text, baseColor, completed);
		}
	}
}
