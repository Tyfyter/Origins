using System;
using Terraria;
using Terraria.UI;

namespace Origins.UI {
	public struct UIDragController(UIElement element) : IBroken {
		static string IBroken.BrokenReason => "Move to PegasusLib";
		bool dragging;
		Vector2 offset;
		public void Update() {
			if (dragging) {
				if (!Main.mouseLeft) {
					dragging = false;
					return;
				}
				element.Left.Pixels = Main.MouseScreen.X + offset.X;
				element.Top.Pixels = Main.MouseScreen.Y + offset.Y;
			}
		}
		public void Click() {
			dragging = true;
			offset = new Vector2(element.Left.Pixels, element.Top.Pixels) - Main.MouseScreen;
		}
		public static Action<UIElement> Attach(Predicate<Vector2> shouldDrag = null, bool clamp = true, bool stopClickThrough = false) {
			return element => Attach(element, shouldDrag, clamp, stopClickThrough);
		}
		public static void Attach(UIElement element, Predicate<Vector2> shouldDrag = null, bool clamp = true, bool stopClickThrough = false) {
			UIDragController dragController = new(element);
			shouldDrag ??= _ => true;
			element.OnLeftMouseDown += (evt, _) => {
				if (shouldDrag(Main.MouseScreen - new Vector2(element.Left.Pixels, element.Top.Pixels))) dragController.Click();
			};
			element.OnUpdate += _ => {
				dragController.Update();
			};
			if (clamp) element.OnUpdate += _ => {
				CalculatedStyle dimensions = element.GetOuterDimensions();
				CalculatedStyle parentDimensions = element.Parent?.GetDimensions() ?? new(0, 0, Main.screenWidth, Main.screenHeight);
				float maxX = parentDimensions.X + parentDimensions.Width - dimensions.Width;
				float maxY = parentDimensions.Y + parentDimensions.Height - dimensions.Height;
				float offsetX = element.HAlign * (dimensions.Width - parentDimensions.Width);
				float offsetY = element.VAlign * (dimensions.Height - parentDimensions.Height);
				Clamp(ref element.Left.Pixels, offsetX, maxX + offsetX);
				Clamp(ref element.Top.Pixels, offsetY, maxY + offsetY);
			};
			if (stopClickThrough) element.OnUpdate += _ => {
				if (element.IsMouseHovering && shouldDrag(Main.MouseScreen - new Vector2(element.Left.Pixels, element.Top.Pixels))) Main.LocalPlayer.mouseInterface = true;
			};
		}
	}
}
