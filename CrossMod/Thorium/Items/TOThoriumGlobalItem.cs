using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.BossMini;
using ThoriumMod.Items.NPCItems;

namespace Origins.CrossMod.Thorium.Items {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			bool statsModified = false;
			if (item?.ModItem?.Mod == OriginsModIntegrations.Thorium) {
				if (item.useAmmo == ItemID.RocketI || item.type == ModContent.ItemType<HandCannon>() || item.type == ModContent.ItemType<MarineLauncher>()) item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			}

			if (statsModified) item.StatsModifiedBy.Add(Mod);
		}
	}
}
