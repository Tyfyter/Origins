using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Player_Head_Handler : ITagHandler {
		public class Player_Head_Snippet(Player player) : TextSnippet {
			readonly Player player = player;
			public Player_Head_Snippet(int WhoAmI) : this(Main.player[WhoAmI]) { }
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (!justCheckingString && (color.R != 0 || color.G != 0 || color.B != 0)) {
					int direction = player.direction;
					Rectangle headFrame = player.headFrame;
					Rectangle hairFrame = player.hairFrame;
					Rectangle bodyFrame = player.bodyFrame;
					Rectangle legFrame = player.legFrame;
					try {
						player.direction = 1;
						player.headFrame.Y = 0;
						player.hairFrame.Y = 0;
						player.bodyFrame.Y = 0;
						player.legFrame.Y = 0;
						Main.PlayerRenderer.DrawPlayerHead(
							Main.Camera,
							player,
							position + new Vector2(11, 0),
							scale: 0.85f * scale
						);
					} finally {
						player.direction = direction;
						player.headFrame = headFrame;
						player.hairFrame = hairFrame;
						player.bodyFrame = bodyFrame;
						player.legFrame = legFrame;
					}
				}
				size = new Vector2(30, 28) * scale;
				return true;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) => new Player_Head_Snippet(int.Parse(text));
	}
}
