using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using System.Runtime.CompilerServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;

namespace Origins.UI; 
public class UIColorPicker(RefGet<Color> color, bool useAlpha = false) : UIPanel {
	readonly ColorChannelInput[] channels = new ColorChannelInput[useAlpha ? 4 : 3];
	public override void OnInitialize() {
		Width.Set(212, 0);
		Height.Set(16, 0);
		ref byte c = ref Unsafe.As<Color, byte>(ref color());
		for (int i = 0; i < channels.Length; i++) {
			Color start = Color.Black;
			Color end = Color.Black;
			Unsafe.AddByteOffset(ref Unsafe.As<Color, byte>(ref start), i) = 0;
			Unsafe.AddByteOffset(ref Unsafe.As<Color, byte>(ref end), i) = 255;
			channels[i] = new(start, end);
			channels[i].Initialize(ref Unsafe.AddByteOffset(ref c, i));
			Height.Pixels += 16;
		}
		UIDragController.Attach(this, new());
		this.StopClickThrough();
		PaddingTop = 8;
	}
	public override void Update(GameTime gameTime) {
		base.Update(gameTime);
		ref byte c = ref Unsafe.As<Color, byte>(ref color());
		Vector2 position = GetInnerDimensions().Position();
		position.X += 64;
		for (int i = 0; i < channels.Length; i++) {
			channels[i].Update(ref Unsafe.AddByteOffset(ref c, i), position);
			position.Y += 16;
		}
		mouseLeftRelease = !Main.mouseLeft;
	}
	public override void Draw(SpriteBatch spriteBatch) {
		this.BackgroundColor = color();
		base.Draw(spriteBatch);
		spriteBatch.Restart();
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		ref byte c = ref Unsafe.As<Color, byte>(ref color());
		Vector2 position = GetInnerDimensions().Position();
		position.X += 64;
		for (int i = 0; i < channels.Length; i++) {
			channels[i].Draw(spriteBatch, position);
			position.Y += 16;
		}
	}
	static bool mouseLeftRelease;
	class ColorChannelInput(Color start, Color end) : ITextInputContainer {
		static readonly Polygon rect = new(
			Vector2.Zero,
			new(100, 0),
			new(100, 16),
			new(0, 16)
		);
		public int CursorIndex { get; set; }
		public bool IsWritingText { get; set; }
		string ITextInputContainer.TextDisplay => IsWritingText ? Text.ToString() : currentValue.ToString();
		public StringBuilder Text { get; } = new();
		byte currentValue;
		public void Initialize(ref byte c) {
			currentValue = c;
			Reset();
		}
		public void Update(ref byte c, Vector2 position) {
			if (!IsWritingText) {
				c = currentValue;
				if (Main.mouseLeft && mouseLeftRelease && Main.MouseScreen.Between(position + new Vector2(100, 0), position + new Vector2(148, 16))) {
					IsWritingText = true;
				}
			}
		}
		public void Draw(SpriteBatch spriteBatch, Vector2 position) {
			if (Main.CurrentInputTextTakerOverride is not null && Main.CurrentInputTextTakerOverride != this) IsWritingText = false;
			if (IsWritingText) {
				this.ProcessInput(out bool typed);
				if (typed && Text.Length > 3) {
					if (CursorIndex > 3) {
						Text.Remove(0, 1);
						CursorIndex--;
					} else {
						Text.Remove(3, 1);
					}
				}
			}
			this.DrawInputContainerText(spriteBatch, position + new Vector2(100, 0), FontAssets.MouseText.Value, Color.Black, IsWritingText, 0.85f);
			rect.vertices[0].Color = rect.vertices[3].Color = start;
			rect.vertices[1].Color = rect.vertices[2].Color = end;
			rect.TranslateTo(position).Draw();
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