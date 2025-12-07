using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Lottery_Ticket : ModItem {
        public string[] Categories => [
            "ExpendableTool"
        ];
        public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<OriginPlayer>().lotteryTicketItem = Item;
		}
	}
}
