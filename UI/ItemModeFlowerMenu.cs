using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;

namespace Origins.UI {
	public abstract class ItemModeFlowerMenu<TMode, TExtraModeData> : ItemModeHUD where TMode : IFlowerMenuItem<TExtraModeData> {
		TMode[] showingModes;
		public abstract IEnumerable<TMode> GetModes();
		public abstract TExtraModeData GetData(TMode mode);
		public virtual void Click(TMode mode) { }
		public override bool ShouldToggle() {
			if (base.ShouldToggle()) {
				showingModes = isShowing ? null : GetModes().ToArray();
				return true;
			}
			if (isShowing && !GetModes().SequenceEqual(showingModes)) return true;
			return false;
		}
		public abstract float DrawCenter();
		protected override void DrawSelf(SpriteBatch spriteBatch) {
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
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						Click(showingModes[i]);
					}
				}
			}
		}
	}
	public interface IFlowerMenuItem<TExtraModeData> {
		public void Draw(Vector2 position, bool hovered, TExtraModeData extraData);
		public bool IsHovered(Vector2 position);
	}
}
