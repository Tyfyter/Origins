using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Journal_Link_Handler : ITagHandler {
		public class Journal_Link_Snippet : TextSnippet {
			string key;
			int lastHovered = 0;
			Flags flags;
			public Journal_Link_Snippet(string key, Color color = default, Flags flags = Flags.NONE) : base() {
				this.key = key;
				Text = Journal_Registry.Entries[key].NameValue;
				CheckForHover = true;
				this.Color = color;
				this.flags = flags;
			}
			public override void Update() {
				base.Update();
				if (lastHovered > 0) lastHovered--;
			}
			public override void OnHover() {
				base.OnHover();
				lastHovered = 4;
				Main.LocalPlayer.mouseInterface = true;
			}
			public override void OnClick() {
				Origins.OpenJournalEntry(key);
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1) {
				const float lightness = 0.95f;
				if (justCheckingString || spriteBatch is null) {
					size = default;
					return false;
				}
				if (flags.HasFlag(Flags.J)) {
					if (lastHovered > 0 || flags.HasFlag(Flags.U)) {
						color = new Color(1f * lightness, 0.8f * lightness, lastHovered * 0.65f * lightness, 1f).MultiplyRGBA(Main.MouseTextColorReal);
						float offset = 1.5f * scale;
						//ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position - Vector2.UnitX * offset, color, 0, Vector2.Zero, new Vector2(scale));
						ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position + Vector2.UnitX * offset, color, 0, Vector2.Zero, new Vector2(scale));
						//ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position - Vector2.UnitY * offset, color, 0, Vector2.Zero, new Vector2(scale));
						Vector2 pos = ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position + Vector2.UnitY * offset, color, 0, Vector2.Zero, new Vector2(scale));
						if (flags.HasFlag(Flags.U)) {
							pos.X += 8;
							pos.Y = position.Y + 2 * scale;
							spriteBatch.Draw(TextureAssets.QuicksIcon.Value, pos, Main.MouseTextColorReal);
						}
					}
					//size = ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
					//ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, new Color(0f, 0f, 0f, color.A / 255f), 0, Vector2.Zero, new Vector2(scale));
					//size = ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
					//size.Y = 0;
					size = default;
					return false;
				}
				if (lastHovered == 0 || lastHovered == 3 || color != Color.Black) {
					size = default;
					return false;
				}
				size = default;
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, new Color(1f * lightness, 0.8f * lightness, lastHovered * 0.15f * lightness, 1f), 0, Vector2.Zero, new Vector2(scale));
				//ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, new Color(0f, 0f, 0f, color.A / 255f), 0, Vector2.Zero, new Vector2(scale));
				//size = ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
				return true;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			Flags flags = default;
			for (int i = 0; i < options.Length; i++) {
				if (Enum.TryParse(options[i].ToString(), true, out Flags flag)) flags |= flag;
			}
			return new Journal_Link_Snippet(text, baseColor, flags);
		}
		public enum Flags {
			NONE = 0,
			/// <summary>
			/// unread
			/// </summary>
			U = 1 << 0,
			/// <summary>
			/// in journal
			/// </summary>
			J = 1 << 1,
		}
	}
}
