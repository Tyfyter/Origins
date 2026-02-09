using System;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Origins.UI {
	public readonly struct CustomNamePlateInfoElement(Func<BestiaryUICollectionInfo, LocalizedText> text) : IBestiaryInfoElement, IProvideSearchFilterString, ICategorizedBestiaryInfoElement {
		public readonly UIBestiaryEntryInfoPage.BestiaryInfoCategory ElementCategory => UIBestiaryEntryInfoPage.BestiaryInfoCategory.Nameplate;
		public readonly UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
			UIElement nameplate = (info.UnlockState != 0) ? new UIText(text(info)) : new UIText("???");
			nameplate.HAlign = 0.5f;
			nameplate.VAlign = 0.5f;
			nameplate.Top = new StyleDimension(2f, 0f);
			nameplate.IgnoresMouseInteraction = true;
			UIElement wrapper = new() {
				Width = new StyleDimension(0f, 1f),
				Height = new StyleDimension(24f, 0f)
			};
			wrapper.Append(nameplate);
			return wrapper;
		}
		public readonly string GetSearchString(ref BestiaryUICollectionInfo info) {
			if (info.UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0) return null;
			return text(info).Value;
		}
	}
}
