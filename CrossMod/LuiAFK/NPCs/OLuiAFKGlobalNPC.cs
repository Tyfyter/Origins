using miningcracks_take_on_luiafk.NPCs;
using Origins.Items.Other.Consumables;
using Origins.Items.Tools;
using Origins.Questing;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.CrossMod.LuiAFK.NPCs {
	[ExtendsFromMod(nameof(miningcracks_take_on_luiafk))]
	public class OLuiAFKGlobalNPC : GlobalNPC {
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == NPCType<SkeletonMerchant>()) {
				shop.Add(ItemID.BlackInk);
				shop.Add<Trash_Lid>(Condition.MoonPhaseFull);
			}
			if (shop.NpcType == NPCType<MobileMerchant>()) {
				shop.Add<Blue_Bovine>(Quest.QuestCondition<Blue_Bovine_Quest>());
				shop.Add<Lottery_Ticket>(Quest.QuestCondition<Lottery_Ticket_Quest>());
			}
		}
	}
}