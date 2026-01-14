using PegasusLib.Reflection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Written to via reflection")]
	public class AccessorySlotLoaderMethods : ReflectionLoader {
		static FastFieldInfo<AccessorySlotLoader, int> slotDrawLoopCounter;
		public static Vector2 CurrentSlotPosition {  get; private set; }
		public override void OnLoad() {
			base.OnLoad();
			MonoModHooks.Add(typeof(AccessorySlotLoader).GetMethod("DrawSlot", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance), DrawSlot);
		}
		delegate void orig_DrawSlot(AccessorySlotLoader self, Item[] items, int context, int slot, bool flag3, int xLoc, int yLoc, bool skipCheck = false);
		delegate void hook_DrawSlot(orig_DrawSlot orig, AccessorySlotLoader self, Item[] items, int context, int slot, bool flag3, int xLoc, int yLoc, bool skipCheck = false);
		static void DrawSlot(orig_DrawSlot orig, AccessorySlotLoader self, Item[] items, int context, int slot, bool flag3, int xLoc, int yLoc, bool skipCheck = false) {
			try {
				CurrentSlotPosition = new(xLoc - 47 * slotDrawLoopCounter.GetValue(self), yLoc);
			} catch (Exception) { }
			orig(self, items, context, slot, flag3, xLoc, yLoc, skipCheck);
		}
	}
}
