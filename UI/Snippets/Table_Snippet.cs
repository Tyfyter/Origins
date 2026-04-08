using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Table_Handler : AdvancedTextSnippetHandler<Table_Handler.Options> {
		public override IEnumerable<string> Names => [
			"ttable"
		];
		public class Table_Snippet : TextSnippet {
			readonly TextSnippet[,][] data;
			readonly float[] width;
			readonly Options options;
			public Table_Snippet(string[,] text, Options options, float scale = 1) : base("", options.Color, scale) {
				data = new TextSnippet[text.GetLength(0), text.GetLength(1)][];
				width = new float[data.GetLength(0)];
				this.options = options;
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				for (int j = 0; j < data.GetLength(1); j++) {
					for (int i = 0; i < data.GetLength(0); i++) {
						if (text[i, j].StartsWith('<') && text[i, j].EndsWith('>')) text[i, j] = '[' + text[i, j][1..^1] + ']';
						data[i, j] = ChatManager.ParseMessage(text[i, j], Color.White).ToArray();
						Max(ref width[i], options.Spacing + ChatManager.GetStringSize(font, data[i, j], Vector2.One).X);
					}
				}
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				size = new(width.Sum(), font.LineSpacing);
				size *= Scale;
				if (justCheckingString || spriteBatch is null) return true;
				for (int i = 0; i < data.GetLength(0); i++) {
					Vector2 currentSize = ChatManager.GetStringSize(font, data[i, options.Line], Vector2.One);
					currentSize.Y = 0;
					position.X += width[i];
					ChatManager.DrawColorCodedString(
						spriteBatch,
						font,
						data[i, options.Line],
						position,
						color,
						0,
						currentSize,
						new(scale * Scale),
						out _,
						-1,
						true
					);
				}
				return true;
			}
		}
		public override Options DefaultOptions => new(Color: Color.White);
		public record struct Options(int Line = 0, float Spacing = 6, Color Color = default);
		public override IEnumerable<SnippetOption> GetOptions() {
			yield return SnippetOption.CreateIntOption("line", value => options.Line = value);
			yield return SnippetOption.CreateFloatOption("sp", value => options.Spacing = value);
			yield return SnippetOption.CreateColorOption("c", value => options.Color = value);
		}
		public override TextSnippet Parse(string text, Color baseColor, Options options) {
			string[] lines = text.Split('|');
			string[][] boxes = new string[lines.Length][];
			boxes[0] = lines[0].Split(',');
			for (int i = 1; i < lines.Length; i++) {
				boxes[i] = lines[i].Split(',');
				if (boxes[i].Length != boxes[0].Length) return new TextSnippet("All lines must have the same number of columns");
			}
			string[,] data = new string[boxes[0].Length, lines.Length];
			for (int i = 0; i < boxes.Length; i++) {
				for (int j = 0; j < boxes[i].Length; j++) {
					data[i, j] = boxes[i][j];
				}
			}
			return new Table_Snippet(data, options);
		}
	}
	/*public class Table_Handler : AdvancedTextSnippetHandler<Table_Handler.Options> {
		public override IEnumerable<string> Names => [
			"table"
		];
		public class Table_Snippet : TextSnippet {
			readonly TextSnippet[,][] data;
			readonly float[] width;
			public Table_Snippet(string[,] text, Options options, float scale = 1) : base("", options.Color, scale) {
				data = new TextSnippet[text.GetLength(0), text.GetLength(1)][];
				width = new float[data.GetLength(0)];
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				for (int j = 0; j < data.GetLength(1); j++) {
					for (int i = 0; i < data.GetLength(0); i++) {
						if (text[i, j].StartsWith('<') && text[i, j].EndsWith('>')) text[i, j] = '[' + text[i, j][1..^1] + ']';
						data[i, j] = ChatManager.ParseMessage(text[i, j], Color).ToArray();
						Max(ref width[i], options.Spacing + ChatManager.GetStringSize(font, data[i, j], Vector2.One).X);
					}
				}
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				size = new(width.Sum(), data.GetLength(1) * font.LineSpacing);
				size *= Scale;
				if (justCheckingString || spriteBatch is null) return true;
				float startingX = position.X;
				for (int j = 0; j < data.GetLength(1); j++) {
					position.X = startingX;
					for (int i = 0; i < data.GetLength(0); i++) {
						Vector2 currentSize = ChatManager.GetStringSize(font, data[i, j], Vector2.One);
						currentSize.Y = 0;
						position.X += font.LineSpacing;
						ChatManager.DrawColorCodedString(
							spriteBatch,
							font,
							data[i, j],
							position,
							color,
							0,
							currentSize,
							new(scale * Scale),
							out _,
							-1
						);
					}
					position.Y += font.LineSpacing;
				}
				return true;
			}
		}
		public override Options DefaultOptions => new();
		public record struct Options(float Spacing = 6, Color Color = default);
		public override IEnumerable<SnippetOption> GetOptions() {
			yield return SnippetOption.CreateFloatOption("sp", value => options.Spacing = value);
			yield return SnippetOption.CreateColorOption("c", value => options.Color = value);
		}
		public override TextSnippet Parse(string text, Color baseColor, Options options) {
			string[] lines = text.Split('|');
			string[][] boxes = new string[lines.Length][];
			boxes[0] = lines[0].Split(',');
			for (int i = 1; i < lines.Length; i++) {
				boxes[i] = lines[i].Split(',');
				if (boxes[i].Length != boxes[0].Length) return new TextSnippet("All lines must have the same number of columns");
			}
			string[,] data = new string[boxes[0].Length, lines.Length];
			for (int i = 0; i < boxes.Length; i++) {
				for (int j = 0; j < boxes[i].Length; j++) {
					data[i, j] = boxes[i][j];
				}
			}
			return new Table_Snippet(data, options);
		}
	}*/
}
