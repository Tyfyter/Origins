using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using Terraria;

namespace Origins.UI {
	public abstract class ItemModeHUD : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.ItemUseHUD.AddState(this);
		public bool isShowing = false;
		public Vector2 activationPosition;
		public float cursorAreaOpacity = 0;
		bool mouseRightRelease = false;
		protected bool RightClicked => Main.mouseRight && mouseRightRelease;
		public virtual bool ShouldToggle() => RightClicked;
		public override void OnEnter() {
			cursorAreaOpacity = 0;
			mouseRightRelease = true;
		}
		public override void OnExit() => isShowing = false;
		public override bool ContainsPoint(Vector2 point) => isShowing && base.ContainsPoint(point);
		public sealed override void Update(GameTime gameTime) {
			if (ShouldToggle()) {
				activationPosition = Main.MouseScreen;
				isShowing = !isShowing;
			}
			mouseRightRelease = !Main.mouseRight;
			if (isShowing) {
				cursorAreaOpacity = 0;
				Update();
				base.Update(gameTime);
			} else {
				MathUtils.LinearSmoothing(ref cursorAreaOpacity, 1, 0.05f);
			}
		}
		public virtual void Update() { }
		public override void Draw(SpriteBatch spriteBatch) {
			if (isShowing) base.Draw(spriteBatch);
			else if (isActive) DrawNearCursor(spriteBatch);
		}
		public abstract void DrawNearCursor(SpriteBatch spriteBatch);
	}
}
