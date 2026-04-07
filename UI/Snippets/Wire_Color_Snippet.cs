using AltLibrary.Common.Systems;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Content;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Wire_Color_Handler : AdvancedTextSnippetHandler<Wire_Color_Handler.Options>, ILoadable, IBrokenContent {
		public override IEnumerable<string> Names => ["wireblock"];
		string IBrokenContent.BrokenReason => "Hardcoded text";
		public class Wire_Color_Snippet(Options options, Color color = default) : WrappingTextSnippet(options.Mode?.Name ?? "Vanilla", color) {
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (options.Name) {
					size = default;
					return false;
				}
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				size = font.MeasureString("█");
				size.Y = font.SpriteCharacters['█'].Padding.Height;
				if (justCheckingString || spriteBatch is null) return true;
				scale *= Scale;
				if (TextUtils.DrawingShadows) {
					if (options.Mode?.MiniWireMenuColor == Color.Black) color = new(color.A, color.A, color.A, color.A);
					spriteBatch.DrawString(font, "█", position, color, 0, Origin, scale, SpriteEffects.None, 0f);
				} else if (options.AllVanilla) {
					//Eek! Sideways code spider!
					spriteBatch.DrawString(font, "▗", position, new Color(253, 254, 83), 0, Origin, scale, SpriteEffects.None, 0f);
					spriteBatch.DrawString(font, "▘", position, new Color(253, 58, 61),  0, Origin, scale, SpriteEffects.None, 0f);
					spriteBatch.DrawString(font, "▖", position, new Color(83, 253, 153), 0, Origin, scale, SpriteEffects.None, 0f);
					spriteBatch.DrawString(font, "▝", position, new Color(83, 180, 253), 0, Origin, scale, SpriteEffects.None, 0f);
				} else {
					spriteBatch.DrawString(font, "█", position, options.Mode.MiniWireMenuColor, 0, Origin, scale, SpriteEffects.None, 0f);
				}
				return true;
			}
		}
		public override IEnumerable<SnippetOption> GetOptions() {
			yield break;
		}
		void ILoadable.Load(Mod mod) {
			Load(mod);
			if (!Main.dedServ) FixBlocks();
		}

		static void FixBlocks() {
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			DynamicSpriteFont.SpriteCharacterData badBlock = font.SpriteCharacters['█'];
			if (badBlock.Glyph != new Rectangle(124, 230, 15, 22)) return;
			if (badBlock.Kerning != new Vector3(1, 15, 1)) return;
			if (badBlock.Padding != new Rectangle(0, -2, 15, 29)) return;
			if (!ModContent.RequestIfExists("Origins/Textures/Chars/Mouse_Text_Blocks", out Asset<Texture2D> asset)) return;
			Task.Run(asset.Wait).ContinueWith(_ => {
				DynamicSpriteFont.SpriteCharacterData GoodBlock(int x = 0, int y = 0) => new(
					asset.Value,
					new(x, y, 16, 19),
					new(0, -2, 16, 29),
					new(1, 16, 1)
				);
				font.SpriteCharacters['█'] = GoodBlock();
				font.SpriteCharacters['▗'] = GoodBlock(34, 20);
				font.SpriteCharacters['▘'] = GoodBlock(17);
				font.SpriteCharacters['▖'] = GoodBlock(17, 20);
				font.SpriteCharacters['▝'] = GoodBlock(34);
			});
		}

		public record struct Options(bool Name, bool AllVanilla, WireMode Mode);
		public override TextSnippet Parse(string text, Color baseColor, Options options) {
			if (text == "Vanilla") {
				options.AllVanilla = true;
			} else if (ModContent.TryFind(text, out WireMode mode)) {
				options.Mode = mode;
			}
			return new Wire_Color_Snippet(options, baseColor);
		}
	}
}
