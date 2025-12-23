using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Journal_Link_Handler : ITagHandler {
		public class Journal_Link_Snippet : TextSnippet {
			public readonly string key;
			public Flags flags;
			int lastHovered = 0;
			public Journal_Link_Snippet(string key, Color color = default, Flags flags = Flags.NONE) : base() {
				this.key = key;
				Text = Journal_Registry.Entries[key].DisplayName.Value;
				if (flags.HasFlag(Flags.L)) {
					Color = Color.Lerp(color, Color.SlateGray, 0.5f);
					CheckForHover = OriginClientConfig.Instance.DebugMenuButton.DebugMode;
				} else {
					Color = color;
					CheckForHover = true;
				}
				this.flags = flags;
			}
			public override void Update() {
				base.Update();
				if (lastHovered > 0) lastHovered--;
			}
			public override void OnHover() {
				if (flags.HasFlag(Flags.L)) return;
				base.OnHover();
				lastHovered = 4;
				Main.LocalPlayer.mouseInterface = true;
			}
			public override void OnClick() {
				if (ItemSlot.ControlInUse && OriginClientConfig.Instance.DebugMenuButton.DebugMode) {
					flags ^= Flags.L;
					if (flags.HasFlag(Flags.L)) OriginPlayer.LocalOriginPlayer.unlockedJournalEntries.Remove(key);
					else OriginPlayer.LocalOriginPlayer.UnlockJournalEntry(key);
					return;
				}
				if (flags.HasFlag(Flags.L)) return;
				Origins.OpenJournalEntry(key);
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
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
							Main.spriteBatch.Draw(
								TextureAssets.QuicksIcon.Value,
								pos,
								null,
								Main.MouseTextColorReal,
								0,
								default,
								scale,
								SpriteEffects.None,
							0);
						}
					}
					if (flags.HasFlag(Flags.L)) {
						Vector2 pos = position + FontAssets.MouseText.Value.MeasureString(Text) * new Vector2(1, 0.35f) * scale;
						pos.X += 4;
						//pos.Y = position.Y + 2 * scale;
						Main.instance.LoadItem(ItemID.ChestLock);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.ChestLock].Value,
							pos,
							null,
							Main.MouseTextColorReal,
							0,
							new(0, TextureAssets.Item[ItemID.ChestLock].Value.Height * 0.5f),
							scale * 0.75f,
							SpriteEffects.None,
						0);
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
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Flags flags = default;
			for (int i = 0; i < options.Length; i++) if (Enum.TryParse(options[i].ToString(), true, out Flags flag)) flags |= flag;
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
			/// <summary>
			/// locked
			/// </summary>
			L = 1 << 2,
		}
	}
	public class Journal_Series_Header_Handler : ITagHandler {
		public class Journal_Series_Header_Snippet : TextSnippet {
			public readonly Flags flags;
			int lastHovered = 0;
			public Journal_Series_Header_Snippet(string text, Color color = default, Flags flags = Flags.NONE) : base(text) {
				if (flags.HasFlag(Flags.L)) Color = Color.Lerp(color, Color.SlateGray, 0.5f);
				else Color = color;
				this.flags = flags;
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				const float lightness = 0.95f;
				if (justCheckingString || spriteBatch is null) {
					size = default;
					return false;
				}
				if (flags.HasFlag(Flags.U)) {
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
				ChatManager.DrawColorCodedString(spriteBatch, StrikethroughFont.Font, Text, position + Vector2.UnitY * FontAssets.MouseText.Value.MeasureString(Text) * 0.25f * scale, color, 0, Vector2.Zero, new Vector2(scale));
				size = default;
				return false;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Flags flags = default;
			for (int i = 0; i < options.Length; i++) if (Enum.TryParse(options[i].ToString(), true, out Flags flag)) flags |= flag;
			return new Journal_Series_Header_Snippet(text, baseColor, flags);
		}
		public enum Flags {
			NONE = 0,
			/// <summary>
			/// unread
			/// </summary>
			U = 1 << 0,
			/// <summary>
			/// locked
			/// </summary>
			L = 1 << 2,
		}
	}
}
