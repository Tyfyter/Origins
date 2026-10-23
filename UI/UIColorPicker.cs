using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using System.Runtime.CompilerServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace Origins.UI; 
public class UIColorPicker(RefGet<Color> color, bool useAlpha = false) : UIElement {
	readonly ColorChannelInput[] channels = new ColorChannelInput[useAlpha ? 4 : 3];
	public override void OnInitialize() {
		Width.Set(367, 0);
		Height.Set(-2, 0);
		ref byte c = ref Unsafe.As<Color, byte>(ref color());
		for (int i = 0; i < channels.Length; i++) {
			Color start = Color.Black;
			Color end = Color.Black;
			Unsafe.AddByteOffset(ref Unsafe.As<Color, byte>(ref start), i) = 0;
			Unsafe.AddByteOffset(ref Unsafe.As<Color, byte>(ref end), i) = 255;
			channels[i] = new(start, end);
			channels[i].Initialize(ref Unsafe.AddByteOffset(ref c, i));
			Height.Pixels += 22;
		}
		PaddingTop = 0;
	}
	public override void LeftMouseDown(UIMouseEvent evt) { }
	public override void Update(GameTime gameTime) {
		base.Update(gameTime);
		ref byte c = ref Unsafe.As<Color, byte>(ref color());
		Vector2 position = GetInnerDimensions().Position();
		position.X += 64;
		for (int i = 0; i < channels.Length; i++) {
			channels[i].Update(ref Unsafe.AddByteOffset(ref c, i), position, mouseLeftRelease);
			position.Y += 22;
		}
		mouseLeftRelease = !Main.mouseLeft;
	}
	public override void Draw(SpriteBatch spriteBatch) {
		//this.BackgroundColor = color();
		base.Draw(spriteBatch);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			this.GetDimensions().ToRectangle() with { Width = 64 },
			color()
		);
		spriteBatch.Restart();
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		ref byte c = ref Unsafe.As<Color, byte>(ref color());
		Vector2 position = GetInnerDimensions().Position();
		position.X += 64;
		for (int i = 0; i < channels.Length; i++) {
			channels[i].Draw(spriteBatch, position);
			position.Y += 22;
		}
	}
	bool mouseLeftRelease;
	class ColorChannelInput(Color start, Color end) : ITextInputContainer {
		static readonly Polygon rect = new(
			Vector2.Zero,
			new(255, 0),
			new(255, 20),
			new(0, 20)
		);
		public int CursorIndex { get; set; }
		public bool IsWritingText { get; set; }
		string ITextInputContainer.TextDisplay => IsWritingText ? Text.ToString() : currentValue.ToString();
		public StringBuilder Text { get; } = new();
		byte currentValue;
		bool isDragging;
		public void Initialize(ref byte c) {
			currentValue = c;
			Reset();
		}
		public void Update(ref byte c, Vector2 position, bool mouseLeftRelease) {
			bool clicked = Main.mouseLeft && mouseLeftRelease;
			bool unclicked = !Main.mouseLeft && !mouseLeftRelease;
			if (clicked && Main.MouseScreen.Between(position, position + new Vector2(255, 16))) isDragging = true;
			if (unclicked && isDragging.TrySet(false)) Reset();
			if (isDragging) currentValue = (byte)(float.Clamp(Main.mouseX - position.X, 0, 255));

			if (!IsWritingText) {
				c = currentValue;
				if (clicked) IsWritingText = Main.MouseScreen.Between(position + new Vector2(255, 0), position + new Vector2(255 + 48, 16));
			}
		}
		public void Draw(SpriteBatch spriteBatch, Vector2 position) {
			if (Main.CurrentInputTextTakerOverride is not null && Main.CurrentInputTextTakerOverride != this) IsWritingText = false;
			if (IsWritingText) {
				this.ProcessInput(out bool typed);
				if (typed && Text.Length > 3) {
					if (CursorIndex > 3) {
						Text.Remove(0, 1);
					} else {
						Text.Remove(3, 1);
					}
				}
				if (CursorIndex > Text.Length) CursorIndex = Text.Length;
			}
			this.DrawInputContainerText(spriteBatch, position + new Vector2(255, 0), FontAssets.MouseText.Value, Color.Black, IsWritingText, 0.85f);
			rect.vertices[0].Color = rect.vertices[3].Color = start;
			rect.vertices[1].Color = rect.vertices[2].Color = end;
			rect.TranslateTo(position).Draw();
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position + new Vector2(currentValue - 2, 0),
				new(0, 0, 2, 20),
				Color.White
			);
		}
		void ITextInputContainer.Submit() {
			if (byte.TryParse(Text.ToString(), out byte v)) {
				currentValue = v;
			}
			Reset();
		}
		public void Reset() {
			IsWritingText = false;
			Text.Clear();
			Text.Append(currentValue);
			CursorIndex = Text.Length;
		}
	}
}