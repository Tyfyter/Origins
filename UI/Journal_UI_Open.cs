﻿using System.Collections.Generic;
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
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.OS;
using Microsoft.Xna.Framework.Input;

namespace Origins.UI {
	public class Journal_UI_Open : UIState {
		public static AutoCastingAsset<Texture2D> BackTexture;
		public static AutoCastingAsset<Texture2D> PageTexture;
		public static AutoCastingAsset<Texture2D> TabsTexture;
		UIElement baseElement;
		List<List<TextSnippet>> pages;
		float yMargin;
		float xMarginOuter;
		float xMarginInner;
		float XMarginTotal => xMarginOuter + xMarginInner;
		int pageOffset = 0;
		int timeSinceSwitch = 0;
		Journal_UI_Mode lastMode = Journal_UI_Mode.Normal_Page;
		Journal_UI_Mode mode = Journal_UI_Mode.Normal_Page;
		ArmorShaderData currentEffect = null;
		Color inkColor;
		public override void OnInitialize() {
			this.RemoveAllChildren();
			baseElement = new UIElement();
			baseElement.Width.Set(0f, 0.55f);
			baseElement.MaxWidth.Set(1600f, 0f);
			baseElement.MinWidth.Set(700f, 0f);
			baseElement.Height.Set(50f, 0.65f);
			baseElement.HAlign = 0.5f;
			baseElement.VAlign = 0.65f;
			Append(baseElement);
			Recalculate();
			xMarginOuter = baseElement.GetDimensions().Width * 0.06f;
			xMarginInner = xMarginOuter * 0.5f;
			yMargin = baseElement.GetDimensions().Height * 0.1f;
			//SetText(loremIpsum);
			SwitchMode((Journal_UI_Mode)OriginClientConfig.Instance.DefaultJournalMode, "");
		}
		public void SetText(string text) => SetText(text, Color.Black);
		public void SetText(string text, Color baseColor) {
			CalculatedStyle bounds = baseElement.GetDimensions();
			bounds.Width = bounds.Width * 0.5f - XMarginTotal;
			bounds.Height -= yMargin * 2.5f;
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 cursor = Vector2.Zero;
			Vector2 result = cursor;
			Vector2 baseScale = Vector2.One;
			float x = font.MeasureString(" ").X;
			float snippetScale;
			float num3 = 0f;
			List<TextSnippet> snippets = ChatManager.ParseMessage(text, baseColor);

			List<List<TextSnippet>> snippetPages = [];
			StringBuilder currentText = new();
			List<TextSnippet> currentPage = [];
			void finishPage() {
				cursor = Vector2.Zero;
				currentPage.Add(new TextSnippet(currentText.ToString(), baseColor));
				currentText.Clear();
				snippetPages.Add(currentPage.ToList());
				currentPage.Clear();
			}
			for (int i = 0; i < snippets.Count; i++) {
				TextSnippet textSnippet = snippets[i];
				if (textSnippet is Journal_Control_Handler.Journal_Control_Snippet ctrl) {
					switch (ctrl.Text.ToLower()) {
						case "end_page":
						finishPage();
						break;
					}
					continue;
				}
				textSnippet.Update();
				snippetScale = textSnippet.Scale;
				Color snippetColor = textSnippet.GetVisibleColor();
				if (textSnippet.UniqueDraw(justCheckingString: true, out var size, Main.spriteBatch, cursor, baseColor, snippetScale)) {
					cursor.X += size.X * baseScale.X * snippetScale;
					currentPage.Add(textSnippet);
					while (cursor.X > bounds.Width) {
						cursor.Y += font.LineSpacing * num3 * baseScale.Y;
						cursor.X -= bounds.Width;
						currentPage.Add(new TextSnippet("\n"));
					}
					result.X = Math.Max(result.X, cursor.X);
					continue;
				}
				if (textSnippet.GetType() != typeof(TextSnippet)) {
					cursor.X += font.MeasureString(textSnippet.Text).X * baseScale.X * snippetScale;
					if (cursor.X > bounds.Width) {
						cursor.Y += font.LineSpacing * num3 * baseScale.Y;
						cursor.X = 0f;
						if (cursor.Y > bounds.Height) {
							finishPage();
						} else {
							currentPage.Add(new TextSnippet("\n"));
						}
					}
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
						if (currentText.Length > 0 || (j > 0 && words[j - 1] == "")) {
							currentText.Append(' ');
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
			pages = snippetPages.ToList();
		}
		public void SwitchMode(Journal_UI_Mode newMode, string key, bool resetPageNumber = true) {
			if (newMode != mode) {
				lastMode = timeSinceSwitch == 0 ? lastMode : mode;
				timeSinceSwitch = 0;
			}
			currentEffect = null;
			if (resetPageNumber) pageOffset = 0;
			switch (mode = newMode) {
				case Journal_UI_Mode.Normal_Page: {
					JournalEntry entry = Journal_Registry.Entries[key];
					currentEffect = entry.TextShader;
					SetText(FormatTags(Language.GetTextValue($"Mods.{entry.Mod.Name}.Journal.{entry.TextKey}.Text")), entry.BaseColor);
				}
				break;

				case Journal_UI_Mode.Index_Page: {
					OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
					StringBuilder builder = new();
					foreach (string entry in Journal_Registry.Entries.Keys) {
						if (!originPlayer.unlockedJournalEntries.Contains(entry)) {
							continue;
						}
						builder.Append($"[j:{entry}]\n");
					}
					SetText(builder.ToString(), Color.Black);
					break;
				}

				case Journal_UI_Mode.Search_Page: {
					//if (!tabLayout) {
					Rectangle bounds = baseElement.GetDimensions().ToRectangle();
					pages = [
							[
								new Journal_Search_Snippet(
									new CalculatedStyle(bounds.X + xMarginOuter,
									bounds.Y + yMargin,
									bounds.Width * 0.5f - XMarginTotal,
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
					List<Quest> activeQuests = [];
					List<Quest> completedQuests = [];
					StringBuilder builder = new();
					foreach (Quest quest in Quest_Registry.Quests) {
						if (quest.ShowInJournal()) {
							if (quest.Completed) {
								completedQuests.Add(quest);
							} else {
								activeQuests.Add(quest);
							}
						}
					}
					for (int i = 0; i < activeQuests.Count; i++) {
						builder.Append($"[q/inJournal:{activeQuests[i].FullName}]\n");
					}
					for (int i = 0; i < completedQuests.Count; i++) {
						builder.Append($"[q/completed,inJournal:{completedQuests[i].FullName}]\n");
					}
					SetText(builder.ToString(), Color.Black);
					break;
				}
				case Journal_UI_Mode.Quest_Page: {
					Quest quest = Quest_Registry.GetQuestByKey(key);
					SetText(FormatTags(quest.GetJournalPage()));
					quest.HasNotification = false;
				}
				break;

				case Journal_UI_Mode.Custom: {
					OriginPlayer.LocalOriginPlayer.journalText ??= [];
					if (OriginPlayer.LocalOriginPlayer.journalText.Count == 0 || !string.IsNullOrWhiteSpace(OriginPlayer.LocalOriginPlayer.journalText[^1])) {
						OriginPlayer.LocalOriginPlayer.journalText.Add("");
					}
					if (OriginPlayer.LocalOriginPlayer.journalText.Count % 2 != 0) {
						OriginPlayer.LocalOriginPlayer.journalText.Add("");
					}
					pages = new(OriginPlayer.LocalOriginPlayer.journalText.Count);
					int dye = 0;
					inkColor = Color.Black;
					if (OriginPlayer.LocalOriginPlayer.journalDye is Item dyeItem && !dyeItem.IsAir) {
						dye = dyeItem.dye;
						if (!ItemID.Sets.NonColorfulDyeItems.Contains(dyeItem.type)) {
							inkColor = Color.White;
						}
					}
					for (int i = 0; i < OriginPlayer.LocalOriginPlayer.journalText.Count; i++) {
						if (i < OriginPlayer.LocalOriginPlayer.journalText.Count) {
							pages.Add(ChatManager.ParseMessage(OriginPlayer.LocalOriginPlayer.journalText[i], inkColor));
						}
					}
					currentEffect = GameShaders.Armor.GetSecondaryShader(dye, Main.LocalPlayer);
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
			List<List<TextSnippet>> snippetPages = [];
			List<TextSnippet> currentPage = [];
			int lineCount = 0;
			if (pages.Count < 1 || pages[0].Count < 0 || pages[0][0] is not Journal_Search_Snippet) {
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
					snippetPages.Add(currentPage.ToList());
					currentPage = [];
					lineCount = 0;
				}
			}
			snippetPages.Add(currentPage.ToList());
			pages = snippetPages;
		}
		public static string[] GetSearchResults(string query) {
			if (string.IsNullOrWhiteSpace(query)) return [];
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
				int tabCount = 4;
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
			spriteBatch.Restart(spriteBatchState, samplerState: SamplerState.PointClamp);
			bool shade = currentEffect is not null;
			bool canDrawArrows = true;
			try {
				if (shade) Origins.shaderOroboros.Capture();
				Quest_Stage_Snippet_Handler.Quest_Stage_Snippet.currentMaxWidth = bounds.Width * 0.5f - XMarginTotal;
				for (int i = 0; i < 2 && i + pageOffset < (pages?.Count ?? 0); i++) {
					Vector2 pagePos = new Vector2(bounds.X + (i * bounds.Width * 0.5f) + (i == 0 ? xMarginOuter : xMarginInner), bounds.Y + yMargin);
					bool canEnterWritingMode = true;
					if (mode == Journal_UI_Mode.Custom && memoPage_focused && i == memoPage_selectedSide) {
						DrawPageForEditing(spriteBatch,
							FontAssets.MouseText.Value,
							pagePos,
							inkColor,
							0,
							Vector2.Zero,
							Vector2.One,
							bounds.Width * 0.5f - XMarginTotal
						);
					} else {
						ChatManager.DrawColorCodedString(spriteBatch,
							FontAssets.MouseText.Value,
							pages[i + pageOffset].ToArray(),
							pagePos,
							Color.White,
							0,
							Vector2.Zero,
							Vector2.One,
							out int hoveredSnippet,
							bounds.Width * 0.5f - XMarginTotal
						);
						if (hoveredSnippet >= 0 && hoveredSnippet < pages[i + pageOffset].Count) {
							if (pages[i + pageOffset][hoveredSnippet] is TextSnippet currentSnippet) {
								if (currentSnippet.CheckForHover) {
									currentSnippet.OnHover();
									if (Main.mouseLeft && Main.mouseLeftRelease) {
										currentSnippet.OnClick();
									}
									if (Main.LocalPlayer.mouseInterface) canEnterWritingMode = false;
								} else if (mode == Journal_UI_Mode.Custom && Main.mouseLeft && Main.mouseLeftRelease) {
									SetMemoFocus(i);
									memoPage_clickPosition = Main.MouseScreen;
								}
							}
						}
					}
					if (mode == Journal_UI_Mode.Custom && canEnterWritingMode && Main.mouseLeft && Main.mouseLeftRelease && (!memoPage_focused || memoPage_selectedSide != i)) {
						if (Main.mouseX > pagePos.X && Main.mouseX < pagePos.X + (bounds.Width * 0.5f - XMarginTotal) && Main.mouseY > pagePos.Y && Main.mouseY < pagePos.Y + bounds.Height) {
							SetMemoFocus(i);
							memoPage_cursorPosition = OriginPlayer.LocalOriginPlayer.journalText[memoPage_selectedSide + pageOffset].Length;
						}
					}
				}
			} finally {
				Quest_Stage_Snippet_Handler.Quest_Stage_Snippet.currentMaxWidth = float.PositiveInfinity;
				if (shade) {
					Origins.shaderOroboros.Stack(currentEffect);
					Origins.shaderOroboros.Release();
				}
			}
			#region arrows
			if (canDrawArrows) {
				if (pageOffset < (pages?.Count ?? 0) - 2) {
					Vector2 position = new Vector2(bounds.X + bounds.Width - xMarginOuter * 0.9f, bounds.Y + bounds.Height - yMargin * 0.9f);
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
					Vector2 position = new Vector2(bounds.X + xMarginOuter * 0.9f, bounds.Y + bounds.Height - yMargin * 0.9f);
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
		int memoPage_selectedSide = -1;
		int memoPage_cursorPosition = 0;
		bool memoPage_focused = false;
		Vector2? memoPage_clickPosition = null;
		public void SetMemoFocus(int side) {
			if (memoPage_focused) SwitchMode(Journal_UI_Mode.Custom, "", false);
			memoPage_selectedSide = side;
			memoPage_focused = side != -1;
		}
		void PasteInMemo(ref string text) {
			string clipboard = Platform.Get<IClipboard>().Value;
			text = text.Insert(memoPage_cursorPosition, clipboard);
			memoPage_cursorPosition += clipboard.Length;
		}
		private Vector2 DrawPageForEditing(SpriteBatch spriteBatch, DynamicSpriteFont font, Vector2 position, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale, float maxWidth) {
			WrappingTextSnippetSetup.SetWrappingData(position, maxWidth);
			int num = -1;
			Vector2 mousePos = Main.MouseScreen;
			Vector2 currentPosition = position;
			Vector2 result = currentPosition;
			float spaceSize = font.MeasureString(" ").X;
			Color color = baseColor;
			float lineShortener = 0f;
			string text = OriginPlayer.LocalOriginPlayer.journalText[memoPage_selectedSide + pageOffset];
			string originalText = text;
			bool exit = false;
			if (memoPage_cursorPosition > text.Length) memoPage_cursorPosition = text.Length;
			if (memoPage_focused) {
				Main.CurrentInputTextTakerOverride = this;
				Main.chatRelease = false;
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string input = Main.GetInputText(" ", allowMultiLine: true);
				if (Main.inputText.PressingControl()) {
					if (JustPressed(Keys.V)) PasteInMemo(ref text);
					else if (JustPressed(Keys.Left)) {
						if (memoPage_cursorPosition <= 0) goto specialControls;
						memoPage_cursorPosition--;
						while (memoPage_cursorPosition > 0 && text[memoPage_cursorPosition - 1] != ' ') {
							memoPage_cursorPosition--;
						}
					} else if (JustPressed(Keys.Right)) {
						if (memoPage_cursorPosition >= text.Length) goto specialControls;
						memoPage_cursorPosition++;
						while (memoPage_cursorPosition < text.Length && text[memoPage_cursorPosition] != ' ') {
							memoPage_cursorPosition++;
						}
					} else if (JustPressed(Keys.Back)) {
						if (memoPage_cursorPosition <= 0) goto specialControls;
						int length = 1;
						memoPage_cursorPosition--;
						while (memoPage_cursorPosition > 0 && text[memoPage_cursorPosition - 1] != ' ') {
							memoPage_cursorPosition--;
							length++;
						}
						text = text.Remove(memoPage_cursorPosition, length);
					}
					goto specialControls;
				}
				if (Main.inputText.PressingShift()) {
					if (JustPressed(Keys.Insert)) {
						PasteInMemo(ref text);
						goto specialControls;
					}
				}
				if (Pressing(Keys.Left)) {
					if (memoPage_cursorPosition > 0) {
						memoPage_cursorPosition--;
					}
				} else if (Pressing(Keys.Right)) {
					if (memoPage_cursorPosition < text.Length) {
						memoPage_cursorPosition++;
					}
				}

				if (input.Length == 0 && memoPage_cursorPosition > 0) {
					text = text.Remove(--memoPage_cursorPosition, 1);
				} else if (input.Length == 2) {
					text = text.Insert(memoPage_cursorPosition++, input[1].ToString());
				} else if (JustPressed(Keys.Delete)) {
					if (memoPage_cursorPosition < text.Length) {
						text = text.Remove(memoPage_cursorPosition, 1);
					}
				}
				if (JustPressed(Keys.Enter)) {
					text = text.Insert(memoPage_cursorPosition++, "\n");
				} else if (Main.inputTextEscape) {
					exit = true;
				}
				specialControls:;
			}
			int bestClickPosMatch = -1;
			float bestClickPosMatchDist = float.PositiveInfinity;
			void DoFindClickPos(int index, Vector2 pos) {
				//Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new(0, 0, 2, 2), Color.Red);
				if (memoPage_clickPosition.HasValue) {
					float distSQ = ((pos - memoPage_clickPosition.Value) * new Vector2(1, 20)).LengthSquared();
					if (distSQ < bestClickPosMatchDist) {
						bestClickPosMatchDist = distSQ;
						bestClickPosMatch = index;
					}
				}
			}
			const char zwnj = '\u200C';//zero-width non-joiner
			List<TextSnippet> snippets = ChatManager.ParseMessage(text.Insert(memoPage_cursorPosition, zwnj.ToString()), color);//sides.SelectMany(t => ChatManager.ParseMessage(t, color)).ToList();
			int charIndex = 0;
			for (int i = 0; i < snippets.Count; i++) {
				TextSnippet textSnippet = snippets[i];
				textSnippet.Update();
				color = textSnippet.GetVisibleColor();
				float scale = textSnippet.Scale;
				if (textSnippet.UniqueDraw(justCheckingString: false, out Vector2 size, spriteBatch, currentPosition, color, baseScale.X * scale)) {
					if (textSnippet is WrappingTextSnippet wrappingSnippet ? wrappingSnippet.IsHovered : mousePos.Between(currentPosition, currentPosition + size)) {
						num = i;
					}
					currentPosition.X += size.X;
					result.X = Math.Max(result.X, currentPosition.X);
					if (maxWidth != -1) {
						float lineSpacing = font.LineSpacing * scale;
						while (currentPosition.X - position.X > maxWidth) {
							currentPosition.X -= maxWidth;
							currentPosition.Y += lineSpacing;
						}
					}
					charIndex += textSnippet.TextOriginal.Length;
					DoFindClickPos(charIndex, currentPosition);
					continue;
				}
				string[] lines = textSnippet.Text.Split('\n');
				bool flag = true;
				for (int lineNum = 0; lineNum < lines.Length; lineNum++) {
					DoFindClickPos(charIndex, currentPosition);
					string line = lines[lineNum];
					string[] array2 = line.Split(' ');
					for (int k = 0; k < array2.Length; k++) {
						if (k != 0) {
							currentPosition.X += spaceSize * baseScale.X * scale;
							DoFindClickPos(++charIndex, currentPosition);
						}
						if (maxWidth > 0f) {
							float wordWidth = font.MeasureString(array2[k]).X * baseScale.X * scale;
							if (currentPosition.X - position.X + wordWidth > maxWidth) {
								currentPosition.X = position.X;
								currentPosition.Y += font.LineSpacing * lineShortener * baseScale.Y;
								result.Y = Math.Max(result.Y, currentPosition.Y);
								lineShortener = 0f;
								DoFindClickPos(charIndex, currentPosition);
							}
						}
						if (lineShortener < scale) {
							lineShortener = scale;
						}
						string[] zwnjSides = array2[k].Split(zwnj);
						if (zwnjSides.Length == 1) {
							spriteBatch.DrawString(font, array2[k], currentPosition, color, rotation, origin, baseScale * textSnippet.Scale * scale, SpriteEffects.None, 0f);
						} else {
							spriteBatch.DrawString(font, zwnjSides[0], currentPosition, color, rotation, origin, baseScale * textSnippet.Scale * scale, SpriteEffects.None, 0f);
							Vector2 cursorOffset = new Vector2(font.MeasureString(zwnjSides[0]).X * baseScale.X * scale, 0);
							Vector2 cursorSize = FontAssets.MouseText.Value.MeasureString("^");
							spriteBatch.DrawString(
								FontAssets.MouseText.Value,
								"^",
								currentPosition + cursorOffset + cursorSize * new Vector2(0f, 0.5f),
								color.MultiplyRGBA(new(0.25f, 0.25f, 0.25f, 0.85f)),
								0,
								cursorSize * Vector2.UnitX * 0.5f,
								scale,
								SpriteEffects.None,
							0);
							spriteBatch.DrawString(font, zwnjSides[1], currentPosition + cursorOffset, color, rotation, origin, baseScale * textSnippet.Scale * scale, SpriteEffects.None, 0f);
						}
						Vector2 vector2 = font.MeasureString(array2[k]);
						if (mousePos.Between(currentPosition, currentPosition + vector2)) {
							num = i;
						}
						currentPosition.X += vector2.X * baseScale.X * scale;
						charIndex += array2[k].Length;
						DoFindClickPos(charIndex, currentPosition);
						result.X = Math.Max(result.X, currentPosition.X);
					}
					if (lineNum < lines.Length - 1 && flag) {
						currentPosition.Y += font.LineSpacing * lineShortener * baseScale.Y;
						currentPosition.X = position.X;
						result.Y = Math.Max(result.Y, currentPosition.Y);
						lineShortener = 0f;
						DoFindClickPos(charIndex, currentPosition);
					}
					flag = true;
					charIndex++;
				}
			}
			if (num >= 0) {
				if (snippets[num] is TextSnippet currentSnippet) {
					currentSnippet.OnHover();
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						currentSnippet.OnClick();
					}
				}
			}
			if (memoPage_clickPosition.HasValue) {
				memoPage_cursorPosition = bestClickPosMatch;
				memoPage_clickPosition = null;
			}
			if (text != originalText) {
				CalculatedStyle bounds = baseElement.GetDimensions();
				if (result.Y - position.Y <= bounds.Height - yMargin * 2f) OriginPlayer.LocalOriginPlayer.journalText[memoPage_selectedSide + pageOffset] = text;
			}
			if (exit) SetMemoFocus(-1);
			return result;
		}
		public static bool JustPressed(Keys key) => Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		Dictionary<Keys, (int count, float rate)> keyRates = [];
		public bool Pressing(Keys key) {
			if (Main.inputText.IsKeyDown(key)) {
				if (!Main.oldInputText.IsKeyDown(key)) {
					keyRates[key] = (15, 7);
					return true;
				}
				if (!keyRates.TryGetValue(key, out (int count, float rate) rateData)) {
					keyRates[key] = rateData = (15, 7);
				}
				rateData.rate -= 0.05f;
				if (rateData.rate < 0f) rateData.rate = 0f;
				if (--rateData.count <= 0) {
					rateData.count = (int)Math.Round(rateData.rate);
					keyRates[key] = rateData;
					return true;
				}
				keyRates[key] = rateData;

			} else if (Main.oldInputText.IsKeyDown(key)) {
				keyRates[key] = (15, 7);
			}
			return false;
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
	public enum Journal_Default_UI_Mode  {
		Index_Page = Journal_UI_Mode.Index_Page,
		Quest_List = Journal_UI_Mode.Quest_List,
		Search_Page = Journal_UI_Mode.Search_Page
	}
}