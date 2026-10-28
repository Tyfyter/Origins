using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Origins.UI; 
public class ToastHandler : UIElement {
	public int toastHeight = 26;
	public int toastDuration = 60 * 6;
	public override void OnInitialize() {
		Width.Set(0, 1);
		Height.Set(0, 1);
		OverflowHidden = true;
		IgnoresMouseInteraction = true;
	}
	public override void Update(GameTime gameTime) {
		for (int i = 0; i < Elements.Count; i++) ((ToastElement)Elements[i]).index = Elements.Count - i;
		base.Update(gameTime);
		for (int i = Elements.Count - 1; i >= 0; i--) {
			if (((ToastElement)Elements[i]).remove) Elements.RemoveAt(i);
		}
	}
	public void Push(string text, Color color = default) {
		if (color == default) color = Color.White;
		ToastElement newToast = new(text, this) {
			TextColor = color
		};
		Append(newToast);
		newToast.Activate();
	}
	class ToastElement(string text, ToastHandler handler) : UITextPanel<string>(text) {
		public int index;
		public bool remove;
		int time = 0;
		public override void OnInitialize() {
			Width.Set(0, 0.75f);
			Height.Set(handler.toastHeight, 0);
			Top.Set(handler.toastHeight, 0);
			HAlign = 0.5f;
			VAlign = 1;
			PaddingTop = 6;
			PaddingBottom = 6;
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			bool ending = ++time >= handler.toastDuration;
			float dest = -((handler.toastHeight + 4) * index);
			if (ending) dest = 0;
			dest += handler.toastHeight;
			float dist = Math.Abs(Top.Pixels - dest);
			bool recalc = Top.Pixels != dest;
			MathUtils.LinearSmoothing(ref Top.Pixels, dest, dist * 0.1f + 1f);
			if (recalc) Recalculate();
			else if (ending) remove = true;
		}
	}
}
