using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Thorium.Items {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalItem : GlobalItem {
		internal static bool isOriginsItemCloningDefaults = false;
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity?.ModItem?.Mod != OriginsModIntegrations.Thorium;
		}
		public override void SetDefaults(Item item) {
			bool statsModified = false;
			if (item.useAmmo == ItemID.RocketI) item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];

			if (statsModified && !isOriginsItemCloningDefaults) item.StatsModifiedBy.Add(Mod);
		}
	}
}
