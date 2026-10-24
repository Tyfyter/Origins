using Microsoft.CodeAnalysis.Differencing;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Structures;
using Origins.Graphics.Primitives;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.UI; 
public class UIColorPicker(RefGet<Color> color, bool useAlpha = false) : UIElement {
	static readonly AdvancedMiscShaderData hueSaturationBoxShader = new(ModContent.Request<Effect>("Origins/Effects/Misc"), "HueSaturationBox");
	readonly ColorChannelInput[] channels = new ColorChannelInput[useAlpha ? 4 : 3];
	Color oldColor = color();
	HSLColor currentHSLColor = color();
	HSLColor oldHSLColor = color();
	UIInputPanel hexPanel;
	public override void OnInitialize() {
		Width.Set(420, 0);
		Height.Set(6, 0);
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
		hexPanel = new() {
			PaddingTop = 6,
			PaddingBottom = 6,
			TextHAlign = 0
		};
		hexPanel.Top.Set(Height.Pixels + 4, 0);
		hexPanel.Left.Set(129, 0);
		hexPanel.Height.Set(16, 0);
		hexPanel.Width.Set(255, 0);
		hexPanel.OnUpdate += HexPanel_OnUpdate;
		hexPanel.OnSubmit += HexPanel_OnSubmit;
		hexPanel.OnExit += UpdateHexCode;
		UpdateHexCode();
		Append(hexPanel);
		Height.Pixels += 16;
		Max(ref Height.Pixels, 110);
		PaddingTop = 0;
	}
	void HexPanel_OnUpdate(UIElement affectedElement) {
		if (Main.mouseLeft && mouseLeftRelease) hexPanel.IsWritingText = hexPanel.IsMouseHovering;
	}
	void HexPanel_OnSubmit(string hex) {
		if (CanParseHex(hex, out Color c)) color() = c;
		hexPanel.Reset();
	}
	void UpdateHexCode() {
		((ITextInputContainer)hexPanel).Text.Clear();
		if (useAlpha) ((ITextInputContainer)hexPanel).Text.Append(color().Hex4());
		else ((ITextInputContainer)hexPanel).Text.Append(color().Hex3());
	}

	public override void LeftMouseDown(UIMouseEvent evt) { }
	public override void Update(GameTime gameTime) {
		base.Update(gameTime);
		ref Color col = ref color();
		ref byte c = ref Unsafe.As<Color, byte>(ref col);
		Vector2 position = GetInnerDimensions().Position();
		position.X += 129;
		position.Y += 10;
		for (int i = 0; i < channels.Length; i++) {
			channels[i].Update(ref Unsafe.AddByteOffset(ref c, i), position, mouseLeftRelease);
			position.Y += 22;
		}
		if (oldColor.TrySet(col)) {
			currentHSLColor = col;
			oldHSLColor = currentHSLColor;
			UpdateHexCode();
		}
		if (oldHSLColor.TrySet(currentHSLColor)) {
			col = currentHSLColor.RGB;
			oldColor = col;
			for (int i = 0; i < channels.Length; i++) channels[i].Initialize(ref Unsafe.AddByteOffset(ref c, i));
			UpdateHexCode();
		}
		mouseLeftRelease = !Main.mouseLeft;
	}
	public override void Draw(SpriteBatch spriteBatch) {
		ref Color col = ref color();
		bool clicked = Main.mouseLeft && mouseLeftRelease;
		bool unclicked = !Main.mouseLeft && !mouseLeftRelease;
		//this.BackgroundColor = color();
		base.Draw(spriteBatch);
		spriteBatch.Restart();
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		Vector2 position = GetInnerDimensions().Position();
		position.Y += 10;
		hueSaturationBoxShader.Apply();
		hueSaturationBox.TranslateTo(position).Draw();
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + new Vector2(currentHSLColor.H, 1 - currentHSLColor.S) * 100f - Vector2.One,
			new Rectangle(0, 0, 2, 2),
			Color.Black
		);
		position.X += 102;

		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		lightnessBar.TranslateTo(position).Draw();

		triangle.TranslateTo(position + new Vector2(19, 50)).FillColor((currentHSLColor with { L = 0.5f }).RGB).Draw();
		bool hoveringHalfLight = isDragging == 0 && triangle.Contains(Main.MouseScreen);
		if (hoveringHalfLight) triangle.ResetColors().DrawOutline();
		triangle.TranslateTo(position + new Vector2(19, 100 * (1 - currentHSLColor.Lightness))).MultiplyColor(Color.Black).Draw();
		triangle.ResetColors().DrawOutline();

		ref byte c = ref Unsafe.As<Color, byte>(ref col);
		position.X += 27;
		for (int i = 0; i < channels.Length; i++) {
			channels[i].Draw(spriteBatch, position);
			position.Y += 22;
		}
		if (clicked) {
			if (hoveringHalfLight) currentHSLColor.L = 0.5f;
			else if (hueSaturationBox.Contains(Main.MouseScreen)) isDragging = 1;
			else if (lightnessBar.Contains(Main.MouseScreen)) isDragging = 2;
		}
		switch (isDragging) {
			case 1:
			(currentHSLColor.H, currentHSLColor.S) = (Main.MouseScreen - hueSaturationBox.TransformedOrigin).Clamp(default, new(100)) / 100;
			currentHSLColor.S = 1 - currentHSLColor.S;
			break;
			case 2:
			currentHSLColor.L = 1 - float.Clamp(Main.MouseScreen.Y - lightnessBar.TransformedOrigin.Y, 0, 100) / 100;
			break;
		}
		if (unclicked) isDragging = 0;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			GetInnerDimensions().ToRectangle() with { Height = 8 },
			col
		);
	}
	bool mouseLeftRelease;
	static readonly Polygon hueSaturationBox = new(
		Vector2.Zero,
		new(100, 0),
		new(100, 100),
		new(0, 100)
	);
	static readonly Polygon lightnessBar = new(
		Vector2.Zero,
		new(18, 0),
		new(18, 100),
		new(0, 100)
	);
	const float half_tri_base = 0.8660254f;
	const float icon_scale = 10 / (0.8660254f * 2);
	static readonly Polygon triangle = new(
		new(0, 0),
		new(5f, 5f),
		new(5f, -5f)
	);
	static readonly Regex canParseHex = new("^(?:[\\da-fA-F]{8}|[\\da-fA-F]{6}|[\\da-fA-F]{3,4})$", RegexOptions.Compiled);
	static bool CanParseHex(string hex, out Color color) {
		if (canParseHex.IsMatch(hex)) {
			color = ParseHex(hex);
			return true;
		}
		color = default;
		return false;
	}
	static Color ParseHex(string hex) {
		int Parse(int index, int size) {
			int startIndex = (index * size);
			return Convert.ToInt32(hex[startIndex..(startIndex + size)], 16);
		}
		switch (hex.Length) {
			case 8:
			return (new Color(Parse(0, 2), Parse(1, 2), Parse(2, 2), Parse(3, 2)));
			case 6:
			return (new Color(Parse(0, 2), Parse(1, 2), Parse(2, 2)));
			case 4:
			return (new Color(Parse(0, 1) * 16 - 1, Parse(1, 1) * 16 - 1, Parse(2, 1) * 16 - 1, Parse(3, 1) * 16 - 1));
			case 3:
			return (new Color(Parse(0, 1) * 16 - 1, Parse(1, 1) * 16 - 1, Parse(2, 1) * 16 - 1));
			default:
			throw new FormatException($"Malformed color code {hex}");
		}
	}
	static UIColorPicker() {
		lightnessBar.vertices[2].Color = Color.Black;
		lightnessBar.vertices[3].Color = Color.Black;
	}
	int isDragging;
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
		bool isHovering;
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

			if (!IsWritingText) c = currentValue;
			isHovering = Main.MouseScreen.Between(position + new Vector2(255, 0), position + new Vector2(255 + 48, 16));
			if (clicked) IsWritingText = isHovering;
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
			/*spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position + new Vector2(255, 0),
				new(0, 0, 48, 16),
				isHovering ? Color.White : Color.Black
			);*/
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