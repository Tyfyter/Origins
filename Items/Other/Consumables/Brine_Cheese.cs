using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Brine_Cheese : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brine Cheese");
			Tooltip.SetDefault("{$CommonItemTooltip.MajorStats}\n'But, there's a catch...'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = BuffID.WellFed3;
			Item.buffTime = 60 * 60 * 12;
			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.Gray;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Rabies, Item.buffTime + 120);
			player.AddBuff(BuffID.Tipsy, Item.buffTime);
			return true;
		}
	}
}
