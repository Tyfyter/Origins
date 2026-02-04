using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Typing_Handler : AdvancedTextSnippetHandler<Typing_Handler.Options> {
		public override IEnumerable<string> Names => [
			"type",
			"typing"
		];
		public class Typing_Snippet(string text, Options options, float scale = 1) : TextSnippet(text, options.Color, scale) {
			readonly StringBuilder DisplayedText = new();
			readonly string OriginalText = text;
			int timer = options.Delay;
			public override void Update() {
				if (DisplayedText.Length < OriginalText.Length && timer.CycleDown(options.Speed)) {
					DisplayedText.Append(OriginalText[DisplayedText.Length]);
					if (options.Clack) SoundEngine.PlaySound(SoundID.MenuTick);
				}
				if (Text.Length != DisplayedText.Length) Text = DisplayedText.ToString();
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				bool displayCursor = options.Cursor;
				if (displayCursor && options.Blink != 0) displayCursor = Main.timeForVisualEffects % (options.Blink * 2) < options.Blink;
				if (!justCheckingString && displayCursor && (options.KeepCursor || DisplayedText.Length < OriginalText.Length)) {
					spriteBatch.DrawString(
						FontAssets.MouseText.Value,
						"_",
						position + new Vector2(1, -3) + FontAssets.MouseText.Value.MeasureString(Text ?? "") * Vector2.UnitX * scale,
						color,
						0,
						new(0, 0),
						new Vector2(0.5f, 1),
						0,
					0);
				}
				return base.UniqueDraw(justCheckingString, out size, spriteBatch, position, color, scale);
			}
		}
		public record struct Options(int Speed = 6, int Delay = 0, int Blink = 20, Color Color = default, bool Cursor = true, bool KeepCursor = false, bool Clack = false);
		public override IEnumerable<SnippetOption> GetOptions() {
			yield return SnippetOption.CreateIntOption("s", speed => options.Speed = speed);
			yield return SnippetOption.CreateIntOption("b", blink => options.Blink = blink);
			yield return SnippetOption.CreateIntOption("d", delay => options.Delay = delay);
			yield return SnippetOption.CreateColorOption("c", color => options.Color = color);
			yield return SnippetOption.CreateFlagOption("cur", () => options.Cursor = true);//TODO: use TextInputContainerExtensions.CursorType
			yield return SnippetOption.CreateFlagOption("k", () => options.KeepCursor = true);
			yield return SnippetOption.CreateFlagOption("cl", () => options.Clack = true);
		}
		public override TextSnippet Parse(string text, Color baseColor, Options options) {
			if (options.Speed == default) options.Speed = 6;
			if (options.Color == default) options.Color = baseColor;
			options.Cursor = !options.Cursor;
			return new Typing_Snippet(text, options);
		}
	}
}
