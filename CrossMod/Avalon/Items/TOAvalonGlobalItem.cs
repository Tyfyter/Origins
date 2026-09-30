using Avalon.Items.Weapons.Magic.Other;
using Avalon.Items.Weapons.Magic.Wands;
using Avalon.ModSupport.MLL.Items;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Avalon.Items {
	[ExtendsFromMod("Avalon")]
	public class TOAvalonGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			try {
				AvalonItem(item);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOAvalonGlobalItem), e)) throw;
			}
		}
		[NoJIT]
		public static void AvalonItem(Item item) {
			bool statsModified = false;
			if (item?.ModItem?.Mod == OriginsModIntegrations.Avalon) {
				if (item.UseVanillaExplosiveAmmo()) {
					item.MakeExplosive();
					statsModified = true;
				}
			}

			if (item.IsType<Boomlash>() || item.IsType<MagicGrenade>()) {
				item.MakeExplosive();
				statsModified = true;
			}

			if (item.IsType<AcidBomb>() || item.IsType<BloodBomb>()) {
				item.ammo = ItemID.Bomb;
				item.MakeExplosive(DamageClasses.ThrownExplosive);
				item.notAmmo = true;
				statsModified = true;
			}

			if (item.IsType<AcidRocket>() || item.IsType<BloodRocket>()) {
				item.MakeExplosive();
				statsModified = true;
			}

			if (statsModified) item.StatsModifiedBy.Add(Origins.instance);
		}
	}
}
