using AltLibrary.Common.Conditions;
using Fargowiltas.Items.Summons.Deviantt;
using Fargowiltas.NPCs;
using Origins.CrossMod.Fargos.Items;
using Origins.Items.Weapons.Ammo;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.NPCs {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class OFargosGlobalNPC : GlobalNPC {
		public override void ModifyShop(NPCShop shop) {
			if (Fargowiltas.Common.Configs.FargoServerConfig.Instance.NPCSales) {
				switch (shop.NpcType) {
					case NPCID.Demolitionist: {
						shop.InsertBefore<Silicon_Ore_Item>(ItemID.Meteorite, Condition.Hardmode);
						shop.InsertAfter<Lost_Ore_Item>(ItemID.CrimtaneOre, Condition.DownedPlantera);
						shop.InsertAfter<Encrusted_Ore_Item>(ItemID.CrimtaneOre, Condition.DownedPlantera);
						shop.InsertAfter<Sanguinite_Ore_Item>(ItemID.CrimtaneOre, Condition.DownedPlantera);
						shop.InsertAfter<Felnum_Ore_Item>(ItemID.CrimtaneOre, Condition.DownedPlantera);
						shop.InsertAfter<Aetherite_Ore_Item>(ItemID.CrimtaneOre, Condition.DownedPlantera);
						shop.InsertAfter<Carburite_Item>(ItemID.CrimtaneOre, Condition.DownedPlantera);
						shop.InsertAfter<Eitrite_Ore_Item>(ItemID.TitaniumOre, Condition.DownedMoonLord);
						break;
					}

					case NPCID.Steampunker: {
						shop.Add<Gray_Solution>(ShopConditions.GetWorldEvilCondition<Defiled_Wastelands_Alt_Biome>());
						shop.Add<Teal_Solution>(ShopConditions.GetWorldEvilCondition<Riven_Hive_Alt_Biome>());
						shop.Add<Orange_Solution>(ShopConditions.GetWorldEvilCondition<Ashen_Alt_Biome>());
						break;
					}

					case NPCID.Dryad: {
						if (shop.NpcType == NPCID.Dryad && shop.TryGetEntry(ItemID.StrangePlant4, out NPCShop.Entry plant)) {
							shop.InsertAfter(plant, new(TileItem.Get<Stardust_Strange_Plant>().Type) { shopCustomPrice = Item.buyPrice(gold: 5) }, Condition.Hardmode);
						}
						break;
					}
				}
			}

			if (shop.NpcType == ModContent.NPCType<LumberJack>() && shop.TryGetEntry(ItemID.Shadewood, out NPCShop.Entry shadewood)) {
				shop.InsertAfter(shadewood, new(ModContent.ItemType<Endowood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
				shop.InsertAfter(shadewood, new(ModContent.ItemType<Marrowick_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
				shop.InsertAfter(shadewood, new(ModContent.ItemType<Artifiber_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
			}

			if (shop.NpcType == ModContent.NPCType<Deviantt>() && shop.TryGetEntry(ModContent.ItemType<CorruptChest>(), out NPCShop.Entry corruptChest) && shop.TryGetEntry(ModContent.ItemType<HallowChest>(), out NPCShop.Entry hallowChest)) {
				Func<bool> predicate = corruptChest.Conditions.FirstOrDefault()?.Predicate ?? (() => true);
				bool evilMimicCon() => predicate() || ProgressFlags.DownedDefiledMimic.IsSet || ProgressFlags.DownedRivenMimic.IsSet || ProgressFlags.DownedTrashCompactorMimic.IsSet;
				corruptChest.Disable();
				shop.GetEntry(ModContent.ItemType<CrimsonChest>()).Disable();

				shop.InsertAfter(hallowChest, new(ModContent.ItemType<CorruptChest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Fargowiltas.Conditions.MimicCorruptDown", evilMimicCon));
				shop.InsertAfter(hallowChest, new(ModContent.ItemType<CrimsonChest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Fargowiltas.Conditions.MimicCrimsonDown", evilMimicCon));

				shop.InsertAfter(hallowChest, new(ModContent.ItemType<Defiled_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedDefiledMimic", evilMimicCon));
				shop.InsertAfter(hallowChest, new(ModContent.ItemType<Riven_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedRivenMimic", evilMimicCon));
				shop.InsertAfter(hallowChest, new(ModContent.ItemType<Suspicious_Trash_Compactor>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedTrashCompactorMimic", evilMimicCon));
			}
		}
	}
}
