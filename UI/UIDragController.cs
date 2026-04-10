using System;
using Terraria;
using Terraria.UI;

namespace Origins.UI {
	public struct UIDragController(UIElement element) : IBroken {
		static string IBroken.BrokenReason => "Move to PegasusLib";
		bool dragging;
		Vector2 offset;
		public void Update(Action drop = null) {
			if (dragging) {
				if (!Main.mouseLeft) {
					dragging = false;
					drop?.Invoke();
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
		public delegate void Modify<T>(ref T value);
		public static Action<UIElement> Attach(AttachParameters parameters) {
			return element => Attach(element, parameters);
		}
		public static Func<bool> Attach(UIElement element, AttachParameters parameters) {
			UIDragController dragController = new(element);
			parameters.ShouldDrag ??= _ => true;
			element.OnLeftMouseDown += (evt, _) => {
				if (!dragController.dragging && parameters.ShouldDrag(Main.MouseScreen - new Vector2(element.Left.Pixels, element.Top.Pixels))) {
					parameters.PickUp?.Invoke();
					dragController.Click();
					parameters.ModifyOffset(ref dragController.offset);
				}
			};
			element.OnUpdate += _ => dragController.Update(parameters.Drop);
			if (parameters.Clamp) element.OnUpdate += _ => {
				CalculatedStyle dimensions = element.GetOuterDimensions();
				CalculatedStyle parentDimensions = element.Parent?.GetDimensions() ?? new(0, 0, Main.screenWidth, Main.screenHeight);
				float maxX = parentDimensions.X + parentDimensions.Width - dimensions.Width;
				float maxY = parentDimensions.Y + parentDimensions.Height - dimensions.Height;
				float offsetX = element.HAlign * (dimensions.Width - parentDimensions.Width);
				float offsetY = element.VAlign * (dimensions.Height - parentDimensions.Height);
				Clamp(ref element.Left.Pixels, offsetX, maxX + offsetX);
				Clamp(ref element.Top.Pixels, offsetY, maxY + offsetY);
			};
			if (parameters.StopClickThrough) element.OnUpdate += _ => {
				if (element.IsMouseHovering && parameters.ShouldDrag(Main.MouseScreen - new Vector2(element.Left.Pixels, element.Top.Pixels))) Main.LocalPlayer.mouseInterface = true;
			};
			return () => dragController.dragging;
		}
		public record struct AttachParameters(
			Predicate<Vector2> ShouldDrag = null,
			Action PickUp = null,
			Modify<Vector2> ModifyOffset = null,
			Action Drop = null,
			bool Clamp = true,
			bool StopClickThrough = false
		);
	}
}
