using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Thorium.Items {
	[ExtendsFromMod("Avalon")]
	public class TOAvalonGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			bool statsModified = false;
			if (item?.ModItem?.Mod == OriginsModIntegrations.Avalon) {
				if (item.UseVanillaExplosiveAmmo()) {
					item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
					statsModified = true;
				}
			}

			if (statsModified) item.StatsModifiedBy.Add(Mod);
		}
	}
}
