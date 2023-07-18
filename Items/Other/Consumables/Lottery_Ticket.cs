using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Lottery_Ticket : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Lottery Ticket");
			// Tooltip.SetDefault("Randomly provides incredible luck while in your inventory\nGets consumed when it works");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 16;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 10);
		}
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<OriginPlayer>().lotteryTicketItem = Item;
		}
	}
}
