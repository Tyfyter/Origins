using System;
using Terraria.GameContent.Bestiary;

namespace Origins.UI; 
public class AprilFoolsNameEntryIcon(int npcNetId, string overrideNameKey = null, Func<string> aprilFoolsFunc = null) : UnlockableNPCEntryIcon(npcNetId, overrideNameKey: overrideNameKey), IEntryIcon {
	public new string GetHoverText(BestiaryUICollectionInfo providedInfo) {
		if (OriginsModIntegrations.CheckAprilFools()) return aprilFoolsFunc();
		return base.GetHoverText(providedInfo);
	}
}
