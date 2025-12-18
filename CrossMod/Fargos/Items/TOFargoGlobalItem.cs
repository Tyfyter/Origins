using Fargowiltas.Items.Explosives;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOFargoGlobalItem : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity.type == ModContent.ItemType<BoomShuriken>();
		}
		public override void SetDefaults(Item item) {
			item.DamageType = DamageClasses.ThrownExplosive;
			item.StatsModifiedBy.Add(Mod);
		}
	}
}
