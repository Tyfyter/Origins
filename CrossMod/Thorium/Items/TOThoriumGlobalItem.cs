using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Items.BossMini;
using ThoriumMod.Items.NPCItems;

namespace Origins.CrossMod.Thorium.Items {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			bool statsModified = false;
			if (item?.ModItem?.Mod == OriginsModIntegrations.Thorium) {
				if (item.UseVanillaExplosiveAmmo() || item.type == ModContent.ItemType<HandCannon>() || item.type == ModContent.ItemType<MarineLauncher>()) {
					item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
					statsModified = true;
				}
			}

			if (statsModified) item.StatsModifiedBy.Add(Mod);
		}
	}
}
