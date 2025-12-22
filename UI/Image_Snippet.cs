using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Image_Handler : ITagHandler {
		internal static ShaderLayerTargetHandler shaderOroboros = new();
		public class Image_Snippet : TextSnippet {
			Asset<Texture2D> image;
			Options options;
			public Image_Snippet(string text, Options options) : base(text, options.Color ?? Color.White, options.Scale) {
				this.options = options;
				if (ModContent.RequestIfExists(text, out image)) {
					Text = "";
				} else {
					Scale = 1;
				}
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (color == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
					color = Color.White;
				} else if (color.A == Main.mouseTextColor) {
					color *= 255f / Main.mouseTextColor;
				}
				size = default;
				if (image is null) return false;
				image.Wait();
				size = (options.Frame?.Size() ?? image.Size()) * Scale * scale;
				if (justCheckingString) return true;
				if (options.Shader != JournalImageShader.None) {
					shaderOroboros.Capture(spriteBatch);
					spriteBatch.Restart(spriteBatch.GetState().FixedCulling());
				}
				spriteBatch?.Draw(image.Value, position, options.Frame, options.Shader != JournalImageShader.None ? Color.White : color, 0, Vector2.Zero, Scale * scale, SpriteEffects.None, 0);
				switch (options.Shader) {
					case JournalImageShader.Sketch:
					Origins.journalDrawingShader.UseSaturation(options.Sharpness);
					Origins.journalDrawingShader.UseColor(color);
					shaderOroboros.Stack(Origins.journalDrawingShader);
					shaderOroboros.Release();
					break;

					case JournalImageShader.Transparent:
					shaderOroboros.Stack(Origins.journalTransparentShader);
					shaderOroboros.Release();
					break;
				}
				return true;
			}
		}
		public record struct Options(JournalImageShader Shader = JournalImageShader.None, float Sharpness = 1, Color? Color = null, float Scale = 1f, Rectangle? Frame = null) {
			public readonly bool Sketch => Shader == JournalImageShader.Sketch;
		}
		record struct SnippetOption(string Name, [StringSyntax(StringSyntaxAttribute.Regex)] string Data, Action<string> Action) {
			public readonly string Pattern => Name + Data;
			public static SnippetOption CreateColorOption(string name, Action<Color> setter) {
				return new(name, "[\\da-fA-F]{3,8}", match => {
					int Parse(int index, int size) {
						int startIndex = (index * size);
						return Convert.ToInt32(match[startIndex..(startIndex + size)], 16);
					}
					switch (match.Length) {
						case 8:
						setter(new Color(Parse(0, 2), Parse(1, 2), Parse(2, 2), Parse(3, 2)));
						break;
						case 6:
						setter(new Color(Parse(0, 2), Parse(1, 2), Parse(2, 2)));
						break;
						case 4:
						setter(new Color(Parse(0, 1) * 16 - 1, Parse(1, 1) * 16 - 1, Parse(2, 1) * 16 - 1, Parse(3, 1) * 16 - 1));
						break;
						case 3:
						setter(new Color(Parse(0, 1) * 16 - 1, Parse(1, 1) * 16 - 1, Parse(2, 1) * 16 - 1));
						break;
						default:
						throw new FormatException($"Malformed color code {match}");
					}
				});
			}
		}
		static void ParseOptions(string optionsText, params SnippetOption[] options) {
			Regex regex = new($"(?:{string.Join("|", options.Select(so => $"({so.Pattern})"))})+");
			GroupCollection groups = regex.Match(optionsText).Groups;
			for (int i = 0; i < groups.Count - 1; i++) {
				string match = groups[i + 1].Value;
				if (match.Length <= 0) continue;
				options[i].Action(match[options[i].Name.Length..]);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Options settings = new(Color: baseColor);
			ParseOptions(options,
				new SnippetOption("sc", "[\\d\\.]+", match => settings.Scale = float.Parse(match)),
				new SnippetOption("fr", "(?:\\d+,){3}\\d+", match => { string[] args = match.Split(','); settings.Frame = new(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3])); }),
				new SnippetOption("s", "[\\d\\.]+", match => settings.Sharpness = float.Parse(match)),
				new SnippetOption("d", "", match => settings.Shader = JournalImageShader.Sketch),
				new SnippetOption("t", "", match => settings.Shader = JournalImageShader.Transparent),
				SnippetOption.CreateColorOption("c", value => settings.Color = value)
			);
			return new Image_Snippet(text, settings);
		}
	}
	public enum JournalImageShader {
		None,
		Sketch,
		Transparent
	}
}