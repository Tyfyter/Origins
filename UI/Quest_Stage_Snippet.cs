using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Quest_Stage_Snippet_Handler : ITagHandler {
		public class Quest_Stage_Snippet : TextSnippet {
			internal static float currentMaxWidth;
			readonly bool completed;
			readonly TextSnippet[] snippets;
			public Quest_Stage_Snippet(string text, Color color, bool completed) {
				Text = text.Replace('<', '[').Replace('>', ']');
				snippets = ChatManager.ParseMessage(Text, color).ToArray();
				this.Color = color;
				this.completed = completed;
			}
			public bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, float maxWidth, Vector2 position = default, Color color = default, float scale = 1) {
				if (snippets.Length > 1) {
					if (justCheckingString || spriteBatch is null) {
						size = new(0, FontAssets.MouseText.Value.LineSpacing * scale);
						for (int i = 0; i < snippets.Length; i++) {
							if (snippets[i].UniqueDraw(true, out Vector2 _size, spriteBatch)) {
								size.X += _size.X;
							} else {
								size.X += FontAssets.MouseText.Value.MeasureString(snippets[i].Text).X;
							}
						}
						if (size.X > maxWidth) {
							size.Y *= (int)((size.X + maxWidth - 1) / maxWidth);
							size.X = maxWidth;
						}
						return true;
					}
					if (completed) color *= 0.666f;
					size = ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, snippets, position, color, 0, Vector2.Zero, new(scale), out int hoveredSnippet, maxWidth, completed) - position;
					if (hoveredSnippet >= 0 && hoveredSnippet < snippets.Length && snippets[hoveredSnippet].CheckForHover) {
						snippets[hoveredSnippet].OnHover();
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							snippets[hoveredSnippet].OnClick();
						}
					}
					if (completed) {
						ChatManager.DrawColorCodedString(spriteBatch, OriginExtensions.StrikethroughFont, snippets, position, new Color(color.R, color.G, color.B, 255), 0, Vector2.Zero, new(scale), out _, maxWidth, completed);
						return true;
					}
					return true;
				}
				if (justCheckingString || spriteBatch is null) {
					size = FontAssets.MouseText.Value.MeasureString(Text);
					return true;
				}
				if (completed) {
					StringBuilder builder = new();
					Vector2 dimensions = FontAssets.MouseText.Value.MeasureString(Text);
					DynamicSpriteFont strikethroughFont = OriginExtensions.StrikethroughFont;
					size = dimensions;
					if (justCheckingString) return false;
					color *= 0.666f;
					ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale), maxWidth);
					ChatManager.DrawColorCodedString(spriteBatch, strikethroughFont, Text, position, new Color(color.R, color.G, color.B, 255), 0, Vector2.Zero, new Vector2(scale), maxWidth);
					//ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, builder.ToString(), position + dimensions * new Vector2(0.5f, 0.025f), new Color(color.R, color.G, color.B, 255), 0, new Vector2(strikeWidth * builder.Length * 0.5f, 0), new Vector2(scale));
					return true;
				} else {
					size = FontAssets.MouseText.Value.MeasureString(Text);
					ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale), maxWidth);
					return true;
				}
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				return UniqueDraw(justCheckingString, out size, spriteBatch, currentMaxWidth, position, color, scale);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			bool completed = options == "completed";
			return new Quest_Stage_Snippet(text, baseColor, completed);
		}
	}
}
