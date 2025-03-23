using Fargowiltas.Items.Summons.Deviantt;
using Fargowiltas.NPCs;
using Origins.CrossMod.Items.Fargos;
using Origins.NPCs;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.CrossMod.NPCs.Fargos {
	[JITWhenModsEnabled("Fargowiltas")]
	public class OFargosGlobalNPC : GlobalNPC {
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == ModContent.NPCType<LumberJack>()) {
				shop.TryGetEntry(ItemID.Shadewood, out NPCShop.Entry entry);

				shop.InsertAfter(entry, new(ModContent.ItemType<Endowood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
				shop.InsertAfter(entry, new(ModContent.ItemType<Marrowick_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
				//shop.InsertAfter(entry, new(ModContent.ItemType<Witherwood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) }, Condition.DrunkWorld);
				//shop.InsertAfter(entry, new(ModContent.ItemType<Eden_Wood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 20) }, Condition.Hardmode);
			}
			if (shop.NpcType == ModContent.NPCType<Deviantt>()) {
				shop.GetEntry(ModContent.ItemType<CorruptChest>()).Conditions.First().Deconstruct(out LocalizedText desc, out Func<bool> pred);
				bool evilMimicCon() => pred.Invoke() || Boss_Tracker.Instance.downedDefiledMimic || Boss_Tracker.Instance.downedRivenMimic || Boss_Tracker.Instance.downedAshenMimic;
				shop.GetEntry(ModContent.ItemType<CorruptChest>()).Disable();
				shop.GetEntry(ModContent.ItemType<CrimsonChest>()).Disable();

				shop.TryGetEntry(ModContent.ItemType<HallowChest>(), out NPCShop.Entry entry);

				shop.InsertAfter(entry, new(ModContent.ItemType<CorruptChest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Fargowiltas.Conditions.MimicCorruptDown", evilMimicCon));
				shop.InsertAfter(entry, new(ModContent.ItemType<CrimsonChest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Fargowiltas.Conditions.MimicCrimsonDown", evilMimicCon));

				shop.InsertAfter(entry, new(ModContent.ItemType<Defiled_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedDefiledMimic", evilMimicCon));
				shop.InsertAfter(entry, new(ModContent.ItemType<Riven_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedRivenMimic", evilMimicCon));/*
				shop.InsertAfter(entry, new(ModContent.ItemType<Ashen_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedAshenMimic", evilMimicCon));*/
			}
		}
	}
}
