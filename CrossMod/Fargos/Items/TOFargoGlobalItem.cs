using Fargowiltas.Items.Explosives;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOFargoGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			try {
				FargosItem(item);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOFargoGlobalItem), e)) throw;
			}
		}
		[NoJIT]
		public static void FargosItem(Item item) {
			bool statsModified = false;
			if (item.IsType<BoomShuriken>()) {
				item.MakeExplosive(DamageClasses.ThrownExplosive);
				statsModified = true;
			}

			if (statsModified) item.StatsModifiedBy.Add(Origins.instance);
		}
	}
}
