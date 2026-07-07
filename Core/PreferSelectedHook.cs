using Terraria;
using Terraria.ModLoader;

namespace Origins.Core;
class PreferSelectedHook : ILoadable {
	void ILoadable.Load(Mod mod) {
		On_Player.QuickGrapple_GetItemToUse += On_Player_QuickGrapple_GetItemToUse;
	}
	static Item On_Player_QuickGrapple_GetItemToUse(On_Player.orig_QuickGrapple_GetItemToUse orig, Player self) {
		if (OriginClientConfig.Instance.PreferSelectedHook && Main.projHook[self.HeldItem.shoot]) return self.HeldItem;
		return orig(self);
	}
	void ILoadable.Unload() { }
}
