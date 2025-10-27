using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;

namespace Origins.UI {
	public abstract class ItemModeFlowerMenu<TMode, TExtraModeData> : ItemModeHUD where TMode : IFlowerMenuItem<TExtraModeData> {
		TMode[] showingModes;
		public abstract IEnumerable<TMode> GetModes();
		public abstract TExtraModeData GetData(TMode mode);
		public virtual void Click(TMode mode) { }
		public sealed override bool ShouldToggle() {
			if (showingModes is null || !GetModes().SequenceEqual(showingModes)) {
				showingModes = GetModes().ToArray();
				return isShowing;
			}
			return Toggle;
		}
		public virtual bool Toggle => base.ShouldToggle();
		public abstract float DrawCenter();
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (showingModes is null) return;
			switch (PlayerInput.CurrentInputMode) {
				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				activationPosition = Main.LocalPlayer.Center.ToScreenPosition();
				break;
			}
			float radius = DrawCenter();
			Max(ref radius, radius * showingModes.Length / 6);
			for (int i = 0; i < showingModes.Length; i++) {
				Vector2 pos = activationPosition + (Vector2.UnitY * -radius).RotatedBy(i * -MathHelper.TwoPi / showingModes.Length);
				bool hovered = showingModes[i].IsHovered(pos);
				showingModes[i].Draw(
					pos,
					hovered,
					GetData(showingModes[i])
				);
				if (hovered) {
					Main.LocalPlayer.mouseInterface = true;
					if ((Main.mouseLeft && Main.mouseLeftRelease) || RightClicked) {
						Click(showingModes[i]);
					}
				}
			}
		}
		public abstract bool GetCursorAreaTexture(TMode mode, out Texture2D texture, out Rectangle? frame, out Color color);
		public override void DrawNearCursor(SpriteBatch spriteBatch) {
			if (showingModes is null) return;
			Vector2 cursorPos = Main.MouseScreen + new Vector2(10 - 9 * PlayerInput.UsingGamepad.ToInt(), 25f);
			float opacity = cursorAreaOpacity;
			if (PlayerInput.UsingGamepad) {
				opacity *= Main.GamepadCursorAlpha;
			}
			List<DrawData> datas = new(showingModes.Length);
			DrawData baseData = new(
				null,
				Vector2.Zero,
				default,
				default,
				0,
				Vector2.One * 5,
				1,
				SpriteEffects.None
			);
			for (int i = 0; i < showingModes.Length; i++) {
				if (GetCursorAreaTexture(showingModes[i], out baseData.texture, out baseData.sourceRect, out baseData.color)) {
					datas.Add(baseData);
				}
			}
			void DrawAt(DrawData data, Vector2 position) {
				data.position = position;
				data.color *= opacity;
				DrawData outline = data;
				outline.sourceRect = (outline.sourceRect ?? Rectangle.Empty) with { X = 0, Y = 0 };
				outline.color = new Color(50, 50, 50) * opacity;
				outline.Draw(spriteBatch);
				data.Draw(spriteBatch);
			}
			if (datas.Count == 2) {
				DrawAt(datas[0], cursorPos - Vector2.UnitX * 6);
				DrawAt(datas[1], cursorPos + Vector2.UnitX * 6);
			} else {
				Vector2 startPos = cursorPos - (Vector2.UnitX * 6 * 0.5f * datas.Count);
				for (int i = 0; i < datas.Count; i++) {
					DrawAt(datas[i], startPos + new Vector2(i * 6, (i % 2) * 12));
				}
			}
		}
	}
	public interface IFlowerMenuItem<TExtraModeData> {
		public void Draw(Vector2 position, bool hovered, TExtraModeData extraData);
		public bool IsHovered(Vector2 position);
	}
}
