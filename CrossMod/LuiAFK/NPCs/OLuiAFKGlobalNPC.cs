using Origins.Items.Other.Consumables;
using Origins.Items.Tools;
using Origins.Questing;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.LuiAFK.NPCs {
	[ExtendsFromMod("miningcracks_take_on_luiafk")]
	public class OLuiAFKGlobalNPC : GlobalNPC {
		public override void ModifyShop(NPCShop shop) {
			if (shop.Name == "LuiAFK Skeleton Merchant Shop") {
				shop.Add(ItemID.BlackInk);
				shop.Add<Trash_Lid>(Condition.MoonPhaseFull);
			}
			if (shop.Name == "LuiAFK Fairy Merchant Shop") {
				shop.Add<Blue_Bovine>(Quest.QuestCondition<Blue_Bovine_Quest>());
				shop.Add<Lottery_Ticket>(Quest.QuestCondition<Lottery_Ticket_Quest>());
			}
		}
	}
}