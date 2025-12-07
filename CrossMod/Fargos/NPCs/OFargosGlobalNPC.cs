using Fargowiltas.Items.Summons.Deviantt;
using Fargowiltas.NPCs;
using Origins.CrossMod.Fargos.Items;
using Origins.NPCs;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.NPCs {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class OFargosGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC?.Mod is Fargowiltas.Fargowiltas;
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == ModContent.NPCType<LumberJack>() && shop.TryGetEntry(ItemID.Shadewood, out NPCShop.Entry shadewood)) {

				shop.InsertAfter(shadewood, new(ModContent.ItemType<Endowood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
				shop.InsertAfter(shadewood, new(ModContent.ItemType<Marrowick_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) });
				//shop.InsertAfter(entry, new(ModContent.ItemType<Witherwood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 15) }, Condition.DrunkWorld);
				//shop.InsertAfter(entry, new(ModContent.ItemType<Eden_Wood_Item>()) { shopCustomPrice = Item.buyPrice(copper: 20) }, Condition.Hardmode);
			}
			if (shop.NpcType == ModContent.NPCType<Deviantt>() && shop.TryGetEntry(ModContent.ItemType<CorruptChest>(), out NPCShop.Entry corruptChest) && shop.TryGetEntry(ModContent.ItemType<HallowChest>(), out NPCShop.Entry hallowChest)) {
				Func<bool> predicate = corruptChest.Conditions.FirstOrDefault()?.Predicate ?? (() => true);
				bool evilMimicCon() => predicate() || Boss_Tracker.Instance.downedDefiledMimic || Boss_Tracker.Instance.downedRivenMimic || Boss_Tracker.Instance.downedAshenMimic;
				corruptChest.Disable();
				shop.GetEntry(ModContent.ItemType<CrimsonChest>()).Disable();

				shop.InsertAfter(hallowChest, new(ModContent.ItemType<CorruptChest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Fargowiltas.Conditions.MimicCorruptDown", evilMimicCon));
				shop.InsertAfter(hallowChest, new(ModContent.ItemType<CrimsonChest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Fargowiltas.Conditions.MimicCrimsonDown", evilMimicCon));

				shop.InsertAfter(hallowChest, new(ModContent.ItemType<Defiled_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedDefiledMimic", evilMimicCon));
				shop.InsertAfter(hallowChest, new(ModContent.ItemType<Riven_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedRivenMimic", evilMimicCon));/*
				shop.InsertAfter(entry, new(ModContent.ItemType<Ashen_Chest>()) { shopCustomPrice = Item.buyPrice(gold: 30) }, new Condition("Mods.Origins.Conditions.DownedAshenMimic", evilMimicCon));*/
			}
		}
	}
}
