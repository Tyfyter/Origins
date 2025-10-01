using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.NPCs;

namespace Origins.CrossMod.Thorium.NPCs {
	[ExtendsFromMod(nameof(ThoriumMod))]
	public class OThoriumGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC?.Mod is ThoriumMod.ThoriumMod;
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == ModContent.NPCType<ConfusedZombie>()) {
				shop.InsertAfter(ItemID.Vertebrae, new Item(ModContent.ItemType<Strange_String>()) {
					shopCustomPrice = Item.buyPrice(0, 0, 7, 50)
				});
				shop.InsertAfter(ItemID.Vertebrae, new Item(ModContent.ItemType<Bud_Barnacle>()) {
					shopCustomPrice = Item.buyPrice(0, 0, 7, 50)
				});
				shop.InsertAfter(ItemID.Vertebrae, new Item(ModContent.ItemType<Biocomponent10>()) {
					shopCustomPrice = Item.buyPrice(0, 0, 7, 50)
				}, Condition.DrunkWorld);
				shop.InsertAfter(ItemID.BloodySpine, new Item(ModContent.ItemType<Nerve_Impulse_Manipulator>()) {
					shopCustomPrice = Item.buyPrice(0, 10)
				}, Condition.Hardmode);
				shop.InsertAfter(ItemID.BloodySpine, new Item(ModContent.ItemType<Sus_Ice_Cream>()) {
					shopCustomPrice = Item.buyPrice(0, 10)
				}, Condition.Hardmode);
				shop.InsertAfter(ItemID.BloodySpine, new Item(ModContent.ItemType<Distress_Beacon>()) {
					shopCustomPrice = Item.buyPrice(0, 10)
				}, Condition.Hardmode, Condition.DrunkWorld);
			}
		}
	}
}
