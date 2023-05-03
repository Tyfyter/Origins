using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Irish_Cheddar : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Irish Cheddar");
			Tooltip.SetDefault("{$CommonItemTooltip.MinorStats}\n'Certainly a little sharp'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 12;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
