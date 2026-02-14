using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;

namespace Origins.UI; 
public class AprilFoolsNameEntryIcon(int npcNetId, string overrideNameKey = null, Func<string> aprilFoolsFunc = null, Action onVisible = null) : UnlockableNPCEntryIcon(npcNetId, overrideNameKey: overrideNameKey), IEntryIcon {
	bool isVisible = false;
	public new void Update(BestiaryUICollectionInfo providedInfo, Rectangle hitbox, EntryIconDrawSettings settings) {
		base.Update(providedInfo, hitbox, settings);
		if (onVisible is null) return;
		if (!settings.IsHovered && UIBestiaryTestMethods._selectedEntryButton.GetValue(Main.BestiaryUI)?.Entry?.Icon != this) isVisible = false;
		else if (isVisible.TrySet(true)) onVisible();
	}
	public new string GetHoverText(BestiaryUICollectionInfo providedInfo) {
		if (OriginsModIntegrations.CheckAprilFools()) return aprilFoolsFunc();
		return base.GetHoverText(providedInfo);
	}
}
