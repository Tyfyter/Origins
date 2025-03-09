using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using ReLogic.Content;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Image_Handler : ITagHandler {
		public class Image_Snippet : TextSnippet {
			Asset<Texture2D> image;
			public Image_Snippet(string text, float scale) : base(text, Color.White, scale) {
				if (ModContent.RequestIfExists(text, out image)) {
					Text = "";
				} else {
					Scale = 1;
				}
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				size = default;
				if (image is null) return false;
				size = image.Size() * Scale;
				spriteBatch?.Draw(image.Value, position, null, Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
				return true;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Regex regex = new("(?:(s[\\d\\.]+))+");
			float scale = 1f;
			foreach (Group group in regex.Match(options).Groups.Values) {
				if (group.Value.Length <= 0) continue;
				switch (group.Value[0]) {
					///scale
					case 's':
					scale = float.Parse(group.Value[1..]);
					break;
				}
			}
			return new Image_Snippet(text, scale);
		}
	}
}