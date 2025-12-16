using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Journal_Search_Snippet : TextSnippet {
		int lastHovered = 0;
		bool focused = false;
		CalculatedStyle dimensions;
		public Journal_Search_Snippet(CalculatedStyle dimensions, Color color = default) : base() {
			Text = "";
			CheckForHover = true;
			this.Color = color;
			this.dimensions = dimensions;
		}
		public override void Update() {
			if (focused) {
				Main.CurrentInputTextTakerOverride = this;
				Main.chatRelease = false;
			}
			base.Update();
			if (lastHovered > 0) lastHovered--;
		}
		public override void OnHover() {
			base.OnHover();
			lastHovered = 4;
			Main.LocalPlayer.mouseInterface = true;
		}
		public override void OnClick() {
			focused = !focused;
			Terraria.GameInput.PlayerInput.WritingText = focused;
		}
		public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1) {
			size = new Vector2(dimensions.Width, dimensions.Height);
			if (justCheckingString) return true;
			if (lastHovered > 0 || focused) {
				if (focused) {
					Main.CurrentInputTextTakerOverride = this;
					Terraria.GameInput.PlayerInput.WritingText = true;
					Main.instance.HandleIME();
					string oldText = Text;
					Text = Main.GetInputText(Text);
					if (Text != oldText && Main.InGameUI.CurrentState is Journal_UI_Open journalUI) {
						journalUI.SetSearchResults(Text);
					}
					if (Main.inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) || Main.inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) {
						focused = false;
					}
				}
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
				return true;
			} else {
				StringBuilder builder = new();
				for (int i = (int)(dimensions.Width / FontAssets.MouseText.Value.MeasureString("_").X); i-- > 0;) {
					builder.Append('_');
				}
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, builder.ToString(), position, color, 0, Vector2.Zero, new Vector2(scale));
				ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, Text, position, color, 0, Vector2.Zero, new Vector2(scale));
			}
			return true;
		}
	}
}
