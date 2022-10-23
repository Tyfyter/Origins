using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using static Tyfyter.Utils.UITools;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.World;
using Terraria.GameContent;
using Terraria.UI.Gamepad;
using Terraria.DataStructures;
using ReLogic.Graphics;
using System.Text.RegularExpressions;
using System.Text;

namespace Origins.UI {
	public class Journal_UI_Open : UIState {
		public static AutoCastingAsset<Texture2D> BackTexture;
		public static AutoCastingAsset<Texture2D> PageTexture;
		UIElement baseElement;
		TextSnippet[][] pages;
		float yMargin;
		float xMargin;
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
			xMargin = baseElement.GetDimensions().Width * 0.1f;
			yMargin = baseElement.GetDimensions().Height * 0.1f;
			SetText(loremIpsum);
		}
		public void SetText(string text) {
			CalculatedStyle bounds = baseElement.GetDimensions();
			bounds.Width = bounds.Width * 0.5f - xMargin * 2;
			bounds.Height -= yMargin * 2;
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 cursor = Vector2.Zero;
			Vector2 result = cursor;
			Vector2 baseScale = Vector2.One;
			float x = font.MeasureString(" ").X;
			float snippetScale;
			float num3 = 0f;
			List<TextSnippet> snippets = ChatManager.ParseMessage(text, Color.White);

			List<TextSnippet[]> snippetPages = new List<TextSnippet[]>();
			StringBuilder currentText = new StringBuilder();
			List<TextSnippet> currentPage = new List<TextSnippet>();
			void finishPage() {
				cursor = Vector2.Zero;
				currentPage.Add(new TextSnippet(currentText.ToString(), Color.Black));
				currentText.Clear();
				snippetPages.Add(currentPage.ToArray());
				currentPage.Clear();
			}
			for (int i = 0; i < snippets.Count; i++) {
				TextSnippet textSnippet = snippets[i];
				textSnippet.Update();
				snippetScale = textSnippet.Scale;
				if (textSnippet.UniqueDraw(justCheckingString: true, out var size, Main.spriteBatch, cursor, default, snippetScale)) {
					cursor.X += size.X * baseScale.X * snippetScale;
					result.X = Math.Max(result.X, cursor.X);
					continue;
				}
				string[] lines = textSnippet.Text.Split('\n');
				bool realLine = true;
				foreach (string line in lines) {
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
					currentPage.Add(new TextSnippet(currentText.ToString(), Color.Black));
					currentText.Clear();
					if (lines.Length > 1 && realLine) {
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
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			SpriteBatchState spriteBatchState = spriteBatch.GetState();
			spriteBatch.Restart(spriteBatchState, samplerState: SamplerState.PointClamp);
			Rectangle bounds = baseElement.GetDimensions().ToRectangle();
			spriteBatch.Draw(BackTexture, bounds, Color.White);
			spriteBatch.Draw(PageTexture, bounds, Color.White);
			int pageCount = pages?.Length??0;
			int pageOffset = 0;
			/*
			ChatManager.DrawColorCodedString(spriteBatch,
					FontAssets.MouseText.Value,
					loremIpsum,
					new Vector2(bounds.X + xMargin, bounds.Y + yMargin),
					Color.Red,
					0,
					Vector2.Zero,
					Vector2.One,
					bounds.Width * 0.5f - xMargin * 2
				);//*/
			for (int i = 0; i < 2 && i + pageOffset < pageCount; i++) {
				ChatManager.DrawColorCodedString(spriteBatch,
					FontAssets.MouseText.Value,
					pages[i + pageOffset],
					new Vector2(bounds.X + (i * bounds.Width * 0.5f) + xMargin, bounds.Y + yMargin),
					Color.Black,
					0,
					Vector2.Zero,
					Vector2.One,
					out _,
					bounds.Width * 0.5f - xMargin * 2
				);
			}
			spriteBatch.Restart(spriteBatchState);
		}
		static string loremIpsum =
@"Dolores rerum odio perferendis aut enim est dicta. Cupiditate et mollitia dolorem. Magni ea laboriosam ad in tempore ab unde doloribus. Aut quam quae id ut consequatur. Quia reiciendis cumque incidunt.

Officia consequatur in dolorem. Et ut porro et ut. Eos omnis aut delectus dolor. Rerum sed non debitis numquam impedit et.

Soluta corrupti delectus quod in quia reiciendis quo nihil. Culpa eum qui nesciunt incidunt officia vitae. Reiciendis et facere voluptatem enim rerum aperiam nihil illo. Consequatur aperiam hic numquam tenetur non est. Culpa deleniti eos sed qui voluptas. Iusto minima aut nostrum voluptates iure et delectus dolore.

Excepturi minus consequuntur ipsum quos. Sit eius soluta nesciunt ipsam odio laudantium aut. Est voluptatibus et animi. Neque reiciendis est laborum quisquam qui amet error sunt. Molestias consequatur odit et quaerat repellendus quia.

Fugiat odio voluptate sunt praesentium consequuntur quia voluptas eum. Facilis molestias doloremque corrupti eaque molestiae illo molestiae. Quaerat velit itaque inventore reprehenderit et itaque. Nam aut rerum animi deleniti sed eius non rem. Iste aliquam architecto ut iste sit repellendus maxime quia.";
	}
}