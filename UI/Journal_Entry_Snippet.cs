using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using Terraria.GameContent;
using Terraria;
using Terraria.UI.Chat;
using Origins.Journal;
using Terraria.Localization;
using System.Drawing;
using System.Collections.Generic;
using System.Security.Policy;

namespace Origins.UI {
	public class Journal_Entry_Handler : ITagHandler {
		public class Journal_Entry_Snippet : WrappingTextSnippet {
			public readonly string key;
			readonly TextSnippet[] snippets;
			public Journal_Entry_Snippet(string key, Color color) {
				this.key = key;
				if (Journal_Registry.Entries.TryGetValue(key, out JournalEntry entry)) {
					Text = Language.GetTextValue($"Mods.{entry.Mod.Name}.Journal.{entry.FullTextKey}.Text").Replace('<', '[').Replace('>', ']');
					List<TextSnippet> _snippets = ChatManager.ParseMessage(Text, color);
					float lineSpace = FontAssets.MouseText.Value.LineSpacing;
					for (int j = 0; j < _snippets.Count; j++) {
						if (j + 1 < _snippets.Count && _snippets[j].UniqueDraw(true, out Vector2 size, null)) {
							size.Y -= lineSpace;
							if (_snippets[j].GetType() != _snippets[j + 1].GetType()) {
								while (size.Y > 0) {
									size.Y -= lineSpace;
									_snippets.Insert(++j, new("\n "));
								}
							}
						}
					}
					snippets = _snippets.ToArray();
				} else {
					Text = key;
					snippets = ChatManager.ParseMessage(Text, color).ToArray();
				}
				this.Color = color;
			}
			public bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, float maxWidth, Vector2 position = default, Color color = default, float scale = 1) {
				size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippets, new(scale), -1);
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
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) => new Journal_Entry_Snippet(options, baseColor);
	}
}
