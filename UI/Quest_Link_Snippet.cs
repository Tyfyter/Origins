using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Questing;
using System;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Quest_Link_Handler : ITagHandler {
		public class Quest_Link_Snippet : TextSnippet {
			readonly string key;
			int lastHovered = 0;
			readonly bool completed;
			readonly bool inJournal;
			public Quest_Link_Snippet(string key, Color color = default, bool completed = false, bool inJournal = false) : this(Quest_Registry.GetQuestByKey(key), color, completed, inJournal) { }
			public Quest_Link_Snippet(Quest quest, Color color = default, bool completed = false, bool inJournal = false) : base() {
				key = quest.FullName;
				Text = quest.NameValue;
				CheckForHover = true;
				Color = color;
				this.completed = completed;
				this.inJournal = inJournal;
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
				if (ItemSlot.ControlInUse && OriginClientConfig.Instance.DebugMenuButton.DebugMode) {
					if (ItemSlot.ShiftInUse) {
						Quest_Registry.GetQuestByKey(key).OnComplete(Main.npc[0]);
					} else {
						Quest_Registry.GetQuestByKey(key).LoadData([]);
					}
					return;
				}
				Origins.OpenJournalQuest(key);
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (inJournal) {
					if (completed) {
						Vector2 dimensions = FontAssets.MouseText.Value.MeasureString(Text);
						ReLogic.Graphics.DynamicSpriteFont strikethroughFont = StrikethroughFont.Font;
						size = dimensions;
						if (justCheckingString) return false;
						color *= 0.666f;
						if (lastHovered > 0) {
							const float lightness = 0.95f;
							for (int i = 0; i < ChatManager.ShadowDirections.Length; i++) {
								Color shadowColor = i == 1 || i == 3 ? color : new Color(1f * lightness, 0.8f * lightness, lastHovered * 0.15f * lightness, 1f);
								ChatManager.DrawColorCodedString(
									spriteBatch,
									FontAssets.MouseText.Value,
									Text,
									position + ChatManager.ShadowDirections[i],
									shadowColor,
									0,
									Vector2.Zero,
									new Vector2(scale)
								);
								ChatManager.DrawColorCodedString(
									spriteBatch,
									strikethroughFont,
									Text,
									position + ChatManager.ShadowDirections[i],
									new Color(shadowColor.R, shadowColor.G, shadowColor.B, 255),
									0,
									new Vector2(0, 0),
									new Vector2(scale)
								);
							}
						}
						ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
						ChatManager.DrawColorCodedString(spriteBatch, strikethroughFont, Text, position, new Color(color.R, color.G, color.B, 255), 0, Vector2.Zero, new Vector2(scale));
						return true;
					} else {
						size = FontAssets.MouseText.Value.MeasureString(Text);
						ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
						if (Quest_Registry.GetQuestByKey(key).HasNotification) {
							Main.spriteBatch.Draw(
								TextureAssets.QuicksIcon.Value,
								position + new Vector2(size.X + 6 * scale, size.Y * scale * 0.3f),
								null,
								Main.MouseTextColorReal,
								0,
								TextureAssets.QuicksIcon.Size() * new Vector2(0.5f, 0.5f),
								scale,
								SpriteEffects.None,
							0);
							/*size = ChatManager.DrawColorCodedStringWithShadow(
								spriteBatch,
								FontAssets.MouseText.Value,
								"!",
								position + new Vector2(size.X + 4 * scale, 0),
								Color.Yellow,
								Color.DarkGoldenrod,
								0,
								Vector2.Zero,
								new Vector2(scale * scaleValue)
							);*/
						}
						return true;
					}
				} else {
					if (justCheckingString || lastHovered == 0 || lastHovered == 2 || spriteBatch is null || color != Color.Black) {
						size = default;
						return false;
					}
					size = default;
					const float lightness = 0.95f;
					ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, new Color(1f * lightness, 0.8f * lightness, lastHovered * 0.15f * lightness, 1f), 0, Vector2.Zero, new Vector2(scale));
					//ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, new Color(0f, 0f, 0f, color.A / 255f), 0, Vector2.Zero, new Vector2(scale));
					//size = ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
					return true;
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string optionString = null) {
			string[] options = (optionString ?? "").Split(',');
			bool completed = false;
			bool inJournal = false;
			for (int i = 0; i < options.Length; i++) {
				switch (options[i]) {
					case "completed":
					completed = true;
					break;

					case "inJournal":
					inJournal = true;
					break;
				}
			}
			return new Quest_Link_Snippet(text, baseColor, completed, inJournal);
		}
	}
}
