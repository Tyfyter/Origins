using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using Terraria.GameContent;
using ReLogic.Graphics;
using System.Text.RegularExpressions;
using System.Text;
using Origins.Journal;
using Terraria.Graphics.Shaders;
using Origins.Questing;
using Origins.Items.Other.Dyes;

namespace Origins.UI {
	public class Journal_UI_Open : UIState {
		public static AutoCastingAsset<Texture2D> BackTexture;
		public static AutoCastingAsset<Texture2D> PageTexture;
		public static AutoCastingAsset<Texture2D> TabsTexture;
		UIElement baseElement;
		TextSnippet[][] pages;
		float yMargin;
		float xMargin;
		int pageOffset = 0;
		int timeSinceSwitch = 0;
		Journal_UI_Mode lastMode = Journal_UI_Mode.Normal_Page;
		Journal_UI_Mode mode = Journal_UI_Mode.Normal_Page;
		ArmorShaderData currentEffect = null;
		const bool tabLayout = true;
		public override void OnInitialize() {
			this.RemoveAllChildren();
			baseElement = new UIElement();
			baseElement.Width.Set(0f, 0.875f);
			baseElement.MaxWidth.Set(900f, 0f);
			baseElement.MinWidth.Set(700f, 0f);
			baseElement.Top.Set(190f, 0f);
			baseElement.Height.Set(-310f, 1f);
			baseElement.HAlign = 0.5f;
			Append(baseElement);
			Recalculate();
			xMargin = baseElement.GetDimensions().Width * 0.075f;
			yMargin = baseElement.GetDimensions().Height * 0.1f;
			//SetText(loremIpsum);
			SwitchMode(Journal_UI_Mode.Index_Page, "");
		}
		public void SetText(string text) => SetText(text, Color.Black);
		public void SetText(string text, Color baseColor) {
			CalculatedStyle bounds = baseElement.GetDimensions();
			bounds.Width = bounds.Width * 0.5f - xMargin * 2;
			bounds.Height -= yMargin * 2.5f;
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 cursor = Vector2.Zero;
			Vector2 result = cursor;
			Vector2 baseScale = Vector2.One;
			float x = font.MeasureString(" ").X;
			float snippetScale;
			float num3 = 0f;
			List<TextSnippet> snippets = ChatManager.ParseMessage(text, baseColor);

			List<TextSnippet[]> snippetPages = [];
			StringBuilder currentText = new();
			List<TextSnippet> currentPage = [];
			void finishPage() {
				cursor = Vector2.Zero;
				currentPage.Add(new TextSnippet(currentText.ToString(), baseColor));
				currentText.Clear();
				snippetPages.Add(currentPage.ToArray());
				currentPage.Clear();
			}
			for (int i = 0; i < snippets.Count; i++) {
				TextSnippet textSnippet = snippets[i];
				textSnippet.Update();
				snippetScale = textSnippet.Scale;
				Color snippetColor = textSnippet.GetVisibleColor();
				if (textSnippet.UniqueDraw(justCheckingString: true, out var size, Main.spriteBatch, cursor, baseColor, snippetScale)) {
					cursor.X += size.X * baseScale.X * snippetScale;
					result.X = Math.Max(result.X, cursor.X);
					currentPage.Add(textSnippet);
					continue;
				}
				string[] lines = textSnippet.Text.Split('\n');
				bool realLine = true;
				int lineNum = 0;
				foreach (string line in lines) {
					lineNum++;
					if (line == "\n") {
						cursor.Y += font.LineSpacing * num3 * baseScale.Y;
						cursor.X = 0f;
						//result.Y = Math.Max(result.Y, cursor.Y);
						currentPage.Add(new TextSnippet("\n"));
						if (cursor.Y > bounds.Height) {
							finishPage();
							//snippetPages.Add(currentPage.ToArray());
							//currentPage.Clear();
						}
						num3 = 0f;
						realLine = false;
						continue;
					}
					string[] words = line.Split(' ');
					for (int j = 0; j < words.Length; j++) {
						if (j != 0) {
							cursor.X += x * baseScale.X * snippetScale;
						}
						float currentWordWidth = font.MeasureString(words[j]).X * baseScale.X * snippetScale;
						if (cursor.X - 0f + currentWordWidth > bounds.Width) {
							cursor.X = 0f;
							cursor.Y += font.LineSpacing * num3 * baseScale.Y;
							//result.Y = Math.Max(result.Y, cursor.Y);
							if (cursor.Y > bounds.Height) {
								finishPage();
								//snippetPages.Add(currentPage.ToArray());
								//currentPage.Clear();
							}
							num3 = 0f;
						}
						if (num3 < snippetScale) {
							num3 = snippetScale;
						}
						Vector2 value = font.MeasureString(words[j]);
						cursor.X += value.X * baseScale.X * snippetScale;
						result.X = Math.Max(result.X, cursor.X);
						if (currentText.Length > 0) {
							currentText.Append(" ");
						}
						currentText.Append(words[j]);
					}
					currentPage.Add(new TextSnippet(currentText.ToString(), snippetColor, snippetScale));
					currentText.Clear();
					if (lines.Length > lineNum && realLine) {
						cursor.Y += font.LineSpacing * num3 * baseScale.Y;
						cursor.X = 0f;
						//result.Y = Math.Max(result.Y, cursor.Y);
						currentPage.Add(new TextSnippet("\n"));
						if (cursor.Y > bounds.Height) {
							finishPage();
							//snippetPages.Add(currentPage.ToArray());
							//currentPage.Clear();
						}
						num3 = 0f;
					}
					realLine = true;
				}
			}
			finishPage();
			pages = snippetPages.ToArray();
		}
		public void SwitchMode(Journal_UI_Mode newMode, string key) {
			lastMode = timeSinceSwitch == 0 ? lastMode : mode;
			timeSinceSwitch = 0;
			currentEffect = null;
			switch (mode = newMode) {
				case Journal_UI_Mode.Normal_Page: {
					JournalEntry entry = Journal_Registry.Entries[key];
					currentEffect = entry.TextShader;
					SetText(FormatTags(Language.GetTextValue($"Mods.{entry.Mod.Name}.Journal.{entry.TextKey}.Text")), entry.BaseColor);
				}
				break;

				case Journal_UI_Mode.Index_Page: {
					List<TextSnippet[]> snippetPages = [];
					List<TextSnippet> currentPage = [];
					int lineCount = 0;
					OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
					foreach (var entry in Journal_Registry.Entries.Keys) {
						if (!originPlayer.unlockedJournalEntries.Contains(entry)) {
							continue;
						}
						currentPage.Add(new Journal_Link_Handler.Journal_Link_Snippet(entry, Color.Black));
						currentPage.Add(new TextSnippet("\n"));
						if (++lineCount > 16) {
							snippetPages.Add(currentPage.ToArray());
							currentPage = [];
							lineCount = 0;
						}
					}
					snippetPages.Add(currentPage.ToArray());
					pages = snippetPages.ToArray();
					break;
				}

				case Journal_UI_Mode.Search_Page: {
					//if (!tabLayout) {
					Rectangle bounds = baseElement.GetDimensions().ToRectangle();
					pages = [
							[
								new Journal_Search_Snippet(
									new CalculatedStyle(bounds.X + xMargin,
									bounds.Y + yMargin,
									bounds.Width * 0.5f - xMargin * 2,
									FontAssets.MouseText.Value.MeasureString("_").Y)
								) {
									Text = key ?? ""
								}
							]
						];
					//}
					SetSearchResults(key);
					break;
				}

				case Journal_UI_Mode.Quest_List: {
					List<TextSnippet[]> snippetPages = [];
					List<TextSnippet> currentPage = [];
					int lineCount = 0;
					List<Quest> activeQuests = [];
					List<Quest> completedQuests = [];
					foreach (var quest in Quest_Registry.Quests) {
						if (quest.ShowInJournal()) {
							if (quest.Completed) {
								completedQuests.Add(quest);
							} else {
								activeQuests.Add(quest);
							}
						}
					}
					for (int i = 0; i < activeQuests.Count; i++) {
						currentPage.Add(new Quest_Link_Handler.Quest_Link_Snippet(activeQuests[i], Color.Black, inJournal: true));
						currentPage.Add(new TextSnippet("\n"));
						if (++lineCount > 16) {
							snippetPages.Add(currentPage.ToArray());
							currentPage = [];
							lineCount = 0;
						}
					}
					/*if (activeQuests.Count > 0 && completedQuests.Count > 0) {
						if (++lineCount >= 16) {
							snippetPages.Add(currentPage.ToArray());
							currentPage = new List<TextSnippet>();
							lineCount = 0;
						} else {
							currentPage.Add(new TextSnippet("______________", Color.Black));
						}
					}*/
					for (int i = 0; i < completedQuests.Count; i++) {
						currentPage.Add(new Quest_Link_Handler.Quest_Link_Snippet(completedQuests[i], new Color(25, 25, 25), true, true));
						currentPage.Add(new TextSnippet("\n"));
						if (++lineCount > 16) {
							snippetPages.Add(currentPage.ToArray());
							currentPage = [];
							lineCount = 0;
						}
					}
					snippetPages.Add(currentPage.ToArray());
					pages = snippetPages.ToArray();
					break;
				}
				case Journal_UI_Mode.Quest_Page: {
					Quest quest = Quest_Registry.GetQuestByKey(key);
					SetText(FormatTags(quest.GetJournalPage()));
					quest.HasNotification = false;
				}
				break;

				case Journal_UI_Mode.Custom: {
					SetText(OriginPlayer.LocalOriginPlayer.journalText);
				}
				break;
			}
		}
		//for example, [i:Origins/Blue_Bovine]
		public static string FormatTags(string text) {
			string outputText = text;
			Regex itemTagRegex = new Regex("(?<=\\[i:)\\S+?(?=:|])");
			Match currentMatch = itemTagRegex.Match(outputText);
			int tries = 1000;
			while (currentMatch is not null && tries-- > 0) {
				if (ModContent.TryFind(currentMatch.Value, out ModItem item)) {
					outputText = itemTagRegex.Replace(outputText, item.Type + "", 1, currentMatch.Index);
				} else if (ItemID.Search.TryGetId(currentMatch.Value, out int id)) {
					outputText = itemTagRegex.Replace(outputText, id + "", 1, currentMatch.Index);
				}
				currentMatch = itemTagRegex.Match(outputText, currentMatch.Index + 1);
			}
			return outputText;
		}
		public void SetSearchResults(string query) {
			List<TextSnippet[]> snippetPages = [];
			List<TextSnippet> currentPage = [];
			int lineCount = 0;
			if (pages.Length < 1 || pages[0].Length < 0 || pages[0][0] is not Journal_Search_Snippet) {
				SwitchMode(Journal_UI_Mode.Search_Page, query);
			}
			currentPage.Add(pages[0][0]);
			currentPage.Add(new TextSnippet("\n"));
			lineCount += 1;
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			foreach (var entry in GetSearchResults(query)) {
				if (!originPlayer.unlockedJournalEntries.Contains(entry)) {
					continue;
				}
				currentPage.Add(new Journal_Link_Handler.Journal_Link_Snippet(entry, Color.Black));
				currentPage.Add(new TextSnippet("\n"));
				if (++lineCount > 16) {
					snippetPages.Add(currentPage.ToArray());
					currentPage = [];
					lineCount = 0;
				}
			}
			snippetPages.Add(currentPage.ToArray());
			pages = snippetPages.ToArray();
		}
		public static string[] GetSearchResults(string query) {
			if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();
			List<(string key, int weight)> entries = [];
			foreach (var item in Journal_Registry.Entries) {
				int index = item.Value.GetQueryIndex(query);
				if (index >= 0) {
					entries.Add((item.Key, index));
				}
			}
			return entries.OrderBy(v => v.weight).Select(v => v.key).ToArray();
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			SpriteBatchState spriteBatchState = spriteBatch.GetState();
			spriteBatch.Restart(spriteBatchState, samplerState: SamplerState.PointClamp);
			Rectangle bounds = baseElement.GetDimensions().ToRectangle();
			spriteBatch.Draw(BackTexture, bounds, Color.White);
			{ // block to put position out of scope everywhere else because I didn't want to have to fix the name conflict some other way
				Vector2 position = default;
				Rectangle frame = default;
				Texture2D texture = TabsTexture;
				float pixelSize = bounds.Width / (256f * 2);
				Journal_UI_Mode? switchMode = null;
				int tabCount = DebugConfig.Instance.DebugMode ? 4 : 3;
				for (int i = 0; i < tabCount; i++) {
					Journal_UI_Mode tabMode = Journal_UI_Mode.Normal_Page;
					bool active = false;
					switch (i) {
						case 0:
						frame = new(2, 66, 70, 30);
						position = bounds.TopLeft() + new Vector2(pixelSize * -24, pixelSize * 31.25f);
						tabMode = Journal_UI_Mode.Index_Page;
						break;

						case 1:
						frame = new(2, 34, 70, 30);
						position = bounds.TopLeft() + new Vector2(pixelSize * -24, pixelSize * 69.25f);
						tabMode = Journal_UI_Mode.Quest_List;
						break;

						case 2:
						frame = new(2, 2, 70, 30);
						position = bounds.BottomLeft() + new Vector2(pixelSize * -24, pixelSize * -61.25f);
						tabMode = Journal_UI_Mode.Search_Page;
						break;

						case 3:
						frame = new(2, 100, 70, 30);
						position = bounds.BottomLeft() + new Vector2(pixelSize * -24, pixelSize * -99.25f);
						tabMode = Journal_UI_Mode.Custom;
						break;
					}
					float pullOut = 0;
					if (tabMode == mode) {
						pullOut = pixelSize * Math.Min(timeSinceSwitch * 3, 12);
						active = true;
					} else if (tabMode == lastMode) {
						pullOut = pixelSize * Math.Max(12 - timeSinceSwitch * 3, 0);
					}
					position.X -= pullOut;
					if (!PlayerInput.IgnoreMouseInterface && new Rectangle((int)position.X, (int)position.Y, (int)(pixelSize * 40 + pullOut), (int)(pixelSize * 30)).Contains(Main.MouseScreen)) {
						active = true;
						Main.LocalPlayer.mouseInterface = true;
						Main.instance.MouseText(Language.GetOrRegister("Mods.Origins.Journal.TabName." + tabMode).Value);
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							switchMode = tabMode;
						}
					}
					spriteBatch.Draw(texture, position, frame, active ? Color.White : Color.LightGray, 0, default, pixelSize, 0, 0);
				}
				if (switchMode.HasValue) {
					SwitchMode(switchMode.Value, "");
				}
			}
			timeSinceSwitch++;
			spriteBatch.Draw(PageTexture, bounds, Color.White);
			int pageCount = pages?.Length ?? 0;
			spriteBatch.Restart(spriteBatchState, samplerState: SamplerState.PointClamp);
			bool shade = currentEffect is not null;
			bool canDrawArrows = true;
			try {
				if (shade) Origins.shaderOroboros.Capture();
				for (int i = 0; i < 2 && i + pageOffset < pageCount; i++) {
					ChatManager.DrawColorCodedString(spriteBatch,
						FontAssets.MouseText.Value,
						pages[i + pageOffset],
						new Vector2(bounds.X + (i * bounds.Width * 0.5f) + xMargin, bounds.Y + yMargin),
						Color.White,
						0,
						Vector2.Zero,
						Vector2.One,
						out int hoveredSnippet,
						bounds.Width * 0.5f - xMargin * 2
					);
					if (hoveredSnippet >= 0 && hoveredSnippet < pages[i + pageOffset].Length) {
						if (pages[i + pageOffset][hoveredSnippet] is TextSnippet currentSnippet) {
							currentSnippet.OnHover();
							if (Main.mouseLeft && Main.mouseLeftRelease) {
								currentSnippet.OnClick();
							}
						}
					}
				}
			} finally {
				if (shade) {
					Origins.shaderOroboros.Stack(currentEffect);
					Origins.shaderOroboros.Release();
				}
			}
			#region arrows
			if (canDrawArrows) {
				if (pageOffset < pageCount - 2) {
					Vector2 position = new Vector2(bounds.X + bounds.Width - xMargin * 0.9f, bounds.Y + bounds.Height - yMargin * 0.9f);
					Rectangle rectangle = new Rectangle((int)position.X - 20, (int)position.Y - 9, 40, 18);
					//temp highlight
					if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							pageOffset += 2;
						}
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position + new Vector2(0, 2),
							null,
							new Color(1f, 1f, 0f, 0f),
							-MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position - new Vector2(0, 2),
							null,
							new Color(1f, 1f, 0f, 0f),
							-MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position + new Vector2(2, 0),
							null,
							new Color(1f, 1f, 0f, 0f),
							-MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position - new Vector2(2, 0),
							null,
							new Color(1f, 1f, 0f, 0f),
							-MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
					}
					spriteBatch.Draw(
						TextureAssets.Item[ItemID.WoodenArrow].Value,
						position,
						null,
						Color.White,
						-MathHelper.PiOver2,
						new Vector2(7, 16),
						1,
						0,
						0
					);
				}
				if (pageOffset > 0) {
					Vector2 position = new Vector2(bounds.X + xMargin * 0.9f, bounds.Y + bounds.Height - yMargin * 0.9f);
					Rectangle rectangle = new Rectangle((int)position.X - 20, (int)position.Y - 9, 40, 18);
					//temp highlight
					if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							pageOffset = Math.Max(pageOffset - 2, 0);
						}
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position + new Vector2(0, 2),
							null,
							new Color(1f, 1f, 0f, 0f),
							MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position - new Vector2(0, 2),
							null,
							new Color(1f, 1f, 0f, 0f),
							MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position + new Vector2(2, 0),
							null,
							new Color(1f, 1f, 0f, 0f),
							MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
						spriteBatch.Draw(
							TextureAssets.Item[ItemID.WoodenArrow].Value,
							position - new Vector2(2, 0),
							null,
							new Color(1f, 1f, 0f, 0f),
							MathHelper.PiOver2,
							new Vector2(7, 16),
							1,
							0,
							0
						);
					}
					spriteBatch.Draw(
						TextureAssets.Item[ItemID.WoodenArrow].Value,
						position,
						null,
						Color.White,
						MathHelper.PiOver2,
						new Vector2(7, 16),
						1,
						0,
						0
					);
				}
			}
			#endregion arrows
			spriteBatch.Restart(spriteBatchState);
		}
		/*
		static string loremIpsum =
@"Dolores rerum odio perferendis aut enim est dicta. Cupiditate et mollitia dolorem. Magni ea laboriosam ad in tempore ab unde doloribus. Aut quam quae id ut consequatur. Quia reiciendis cumque incidunt.

Officia consequatur in dolorem. Et ut porro et ut. Eos omnis aut delectus dolor. Rerum sed non debitis numquam impedit et.

Soluta corrupti delectus quod in quia reiciendis quo nihil. Culpa eum qui nesciunt incidunt officia vitae. Reiciendis et facere voluptatem enim rerum aperiam nihil illo. Consequatur aperiam hic numquam tenetur non est. Culpa deleniti eos sed qui voluptas. Iusto minima aut nostrum voluptates iure et delectus dolore.

Excepturi minus consequuntur ipsum quos. Sit eius soluta nesciunt ipsam odio laudantium aut. Est voluptatibus et animi. Neque reiciendis est laborum quisquam qui amet error sunt. Molestias consequatur odit et quaerat repellendus quia.

Fugiat odio voluptate sunt praesentium consequuntur quia voluptas eum. Facilis molestias doloremque corrupti eaque molestiae illo molestiae. Quaerat velit itaque inventore reprehenderit et itaque. Nam aut rerum animi deleniti sed eius non rem. Iste aliquam architecto ut iste sit repellendus maxime quia.";
		*/
	}
	public enum Journal_UI_Mode {
		Normal_Page,
		Index_Page,
		Search_Page,
		Quest_List,
		Quest_Page,
		Custom,
		INVALID
	}
}