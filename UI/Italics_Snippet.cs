using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Italics_Snippet_Handler : ITagHandler {
		public class Italics_Snippet : WrappingTextSnippet {
			internal static float currentMaxWidth;
			readonly TextSnippet[] snippets;
			readonly float amount;
			public Italics_Snippet(string text, Color color, float amount) {
				Text = text.Replace('<', '[').Replace('>', ']');
				if (color == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255)) {
					color = Color.White;
				}
				this.Color = color;
				this.amount = amount;
			}
			public bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, float maxWidth, Vector2 position = default, Color color = default, float scale = 1) {
				if (color.A == Main.mouseTextColor) {
					color *= 255f / Main.mouseTextColor;
				}
				TextSnippet[] snippets = ChatManager.ParseMessage(Text, color).ToArray();
				size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, new(scale), -1) + new Vector2(7 * Math.Abs(amount) * scale, 0);
				if (justCheckingString || spriteBatch is null) return true;

				TextSnippet[] _snippets = new TextSnippet[snippets.Length + 1];
				float padding = position.X - BasePosition.X;
				_snippets[0] = new PaddingSnippet(padding);
				snippets.CopyTo(_snippets, 1);
				position.X = BasePosition.X;
				SpriteBatchState state = spriteBatch.GetState();
				int hoveredSnippet = -1;
				try {
					Matrix sheared = state.transformMatrix;
					sheared.Down += new Vector3(amount, 0, 0);
					sheared.Translation += new Vector3((position.Y + 14) * amount, 0, 0);
					if (amount < 0) position.X -= 7 * amount;
					spriteBatch.Restart(state, transformMatrix: sheared);
					ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, _snippets, position, Color.White, 0, Vector2.Zero, new(scale), out hoveredSnippet, maxWidth);
				} finally {
					spriteBatch.Restart(state);
				}
				if (hoveredSnippet >= 0 && hoveredSnippet < _snippets.Length && _snippets[hoveredSnippet].CheckForHover) {
					_snippets[hoveredSnippet].OnHover();
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						_snippets[hoveredSnippet].OnClick();
					}
				}
				return true;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				return UniqueDraw(justCheckingString, out size, spriteBatch, currentMaxWidth, position, color, scale);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			float amount = 0.5f;
			SnippetHelper.ParseOptions(options,
				SnippetOption.CreateFloatOption("a", value => amount = value)
			);
			return new Italics_Snippet(text, baseColor, amount);
		}
	}
}
