using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static ReLogic.Graphics.DynamicSpriteFont;

namespace Origins.UI {
	public class Wiggle_Handler : ITagHandler {
		internal static Vector2 origin;
		public class Wiggle_Snippet(string text, Options options, Color color = default, float scale = 1) : TextSnippet(text, color, scale) {
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (justCheckingString || spriteBatch is null) {
					size = default;
					return false;
				}
				size = FontAssets.MouseText.Value.MeasureString(Text) * scale * Vector2.UnitX;
				static SpriteCharacterData GetCharacterData(char character) {
					if (!FontAssets.MouseText.Value.SpriteCharacters.TryGetValue(character, out SpriteCharacterData value)) {
						return FontAssets.MouseText.Value.DefaultCharacterData;
					}
					return value;
				}
				Vector2 zero = default;
				foreach (char c in Text) {
					SpriteCharacterData characterData = GetCharacterData(c);
					Vector3 kerning = characterData.Kerning;
					Rectangle padding = characterData.Padding;
					zero.X += FontAssets.MouseText.Value.CharacterSpacing * scale;
					zero.X += kerning.X * scale;
					Vector2 pos = zero;
					pos.X += padding.X * scale;
					pos.Y += padding.Y * scale;
					pos.Y += (float)Math.Sin(zero.X / (options.WiggleWidth * scale) + Main.timeForVisualEffects * options.Speed) * options.WiggleScale;
					spriteBatch.Draw(characterData.Texture, pos + position, characterData.Glyph, color, 0, origin, scale, default, 0);
					zero.X += (kerning.Y + kerning.Z) * scale;
				}
				return true;
			}
		}
		public record struct Options(float Speed = 1f / 60f, float WiggleWidth = 16, float WiggleScale = 2);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Options snoptions = new(WiggleWidth: 16);
			SnippetHelper.ParseOptions(options,
				SnippetOption.CreateFloatOption("t", v => snoptions.Speed = v),
				SnippetOption.CreateFloatOption("x", v => snoptions.WiggleWidth = v),
				SnippetOption.CreateFloatOption("y", v => snoptions.WiggleScale = v)
			);
			return new Wiggle_Snippet(text, snoptions, baseColor, 1);
		}
	}
}
