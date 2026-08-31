using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;

namespace Origins.Core; 
internal class FontGenerator {
	/*public static Asset<DynamicSpriteFont> Monospace(Asset<DynamicSpriteFont> baseFont, float spacing, int lineSpacing, char defaultCharacter) {
		Task loadTask;
		Task.Run(baseFont.Wait).ContinueWith(_ => {

		});
	}*/
	public static DynamicSpriteFont Monospace(DynamicSpriteFont baseFont, float spacing, int lineSpacing, char defaultCharacter, params Span<CharRange> ranges) {
		DynamicSpriteFont newFont = new(spacing, lineSpacing, defaultCharacter);
		if (baseFont is not null) {
			foreach ((char c, DynamicSpriteFont.SpriteCharacterData data) in baseFont.SpriteCharacters) {
				newFont.SpriteCharacters[c] = data;
			}
		}
		foreach (CharRange range in ranges) {
			(Texture2D Texture, Range _range, Rectangle glyph, Rectangle padding, Vector3 kerning) = range;
			char end = (char)_range.End.Value;
			for (char i = (char)_range.Start.Value; i <= end; i++) {
				newFont.SpriteCharacters[i] = new(Texture, glyph, padding, kerning);
				if (i == end) break;
				glyph.X += range.GlyphXChange;
				glyph.Y += range.GlyphYChange;
				glyph.Width += range.GlyphWidthChange;
				glyph.Height += range.GlyphHeightChange;
				padding.X += range.PaddingXChange;
				padding.Y += range.PaddingYChange;
				padding.Width += range.PaddingWidthChange;
				padding.Height += range.PaddingHeightChange;
				kerning += range.KerningChange;
			}
		}
		return newFont;
	}
	public record struct CharRange(Texture2D Texture, Range Range, Rectangle StartGlyph, Rectangle StartPadding, Vector3 StartKerning) {
		public int GlyphXChange { get; set; }
		public int GlyphYChange { get; set; }
		public int GlyphWidthChange { get; set; }
		public int GlyphHeightChange { get; set; }
		public int PaddingXChange { get; set; }
		public int PaddingYChange { get; set; }
		public int PaddingWidthChange { get; set; }
		public int PaddingHeightChange { get; set; }
		public Vector3 KerningChange { get; set; }
	}
}
