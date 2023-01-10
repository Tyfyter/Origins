using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Tasty_Vanilla_Shake : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Vanilla Shake");
			Tooltip.SetDefault("{$CommonItemTooltip.MediumStats}");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ale);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = BuffID.WellFed2;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}
	}
}
