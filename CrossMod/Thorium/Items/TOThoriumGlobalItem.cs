using System;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Items.BossMini;
using ThoriumMod.Items.NPCItems;

namespace Origins.CrossMod.Thorium.Items {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			try {
				ThoriumItem(item);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOThoriumGlobalItem), e)) throw;
			}
		}
		[NoJIT]
		public static void ThoriumItem(Item item) {
			bool statsModified = false;
			if (item?.ModItem?.Mod == OriginsModIntegrations.Thorium) {
				if (item.UseVanillaExplosiveAmmo() || item.IsType<HandCannon>() || item.IsType<MarineLauncher>()) {
					item.MakeExplosive();
					statsModified = true;
				}
			}

			if (statsModified) item.StatsModifiedBy.Add(Origins.instance);
		}
	}
}
