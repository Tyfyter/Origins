using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public abstract class Lottery_Ticket : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brine-Leafed Clover");
			Tooltip.SetDefault("Randomly increases the amount of a dropped item while in your inventory\nGets consumed when it works");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 28);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 10);
		}
		public override void UpdateEquip(Player player) {
			/*OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.lotteryTicket;*/
		}
	}
}
