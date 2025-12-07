using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.NPCs;
using static Terraria.ModLoader.ModContent;

namespace Origins.CrossMod.Thorium.NPCs {
	[ExtendsFromMod(nameof(ThoriumMod))]
	public class OThoriumGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC?.Mod is ThoriumMod.ThoriumMod;
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == NPCType<ConfusedZombie>()) {
				shop.InsertAfter(ItemID.Vertebrae, new Item(ItemType<Strange_String>()) { shopCustomPrice = Item.buyPrice(0, 0, 7, 50) });
				shop.InsertAfter(ItemID.Vertebrae, new Item(ItemType<Bud_Barnacle>()) { shopCustomPrice = Item.buyPrice(0, 0, 7, 50) });
				shop.InsertAfter(ItemID.Vertebrae, new Item(ItemType<Biocomponent10>()) { shopCustomPrice = Item.buyPrice(0, 0, 7, 50) });

				shop.InsertAfter(ItemID.BloodySpine, new Item(ItemType<Nerve_Impulse_Manipulator>()) { shopCustomPrice = Item.buyPrice(0, 10) }, Condition.Hardmode);
				shop.InsertAfter(ItemID.BloodySpine, new Item(ItemType<Sus_Ice_Cream>()) { shopCustomPrice = Item.buyPrice(0, 10) }, Condition.Hardmode);
				shop.InsertAfter(ItemID.BloodySpine, new Item(ItemType<Distress_Beacon>()) { shopCustomPrice = Item.buyPrice(0, 10) }, Condition.Hardmode);
			}
			if (shop.NpcType == NPCType<Druid>()) {
				shop.InsertAfter(ItemID.DeathweedSeeds, new Item(ItemType<Wilting_Rose_Seeds>()) { shopCustomPrice = Item.buyPrice(silver: 10) }, Condition.DownedEowOrBoc);
				shop.InsertAfter(ItemID.DeathweedSeeds, new Item(ItemType<Surveysprout_Seeds>()) { shopCustomPrice = Item.buyPrice(silver: 10) }, Condition.DownedEowOrBoc);
			}
		}
	}
}
