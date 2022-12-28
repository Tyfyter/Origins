using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class One_Kind_Cookie : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("One of a Kind Cookie");
			Tooltip.SetDefault("Feel the cookie...");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ChocolateChipCookie);
			Item.buffType = BuffID.WellFed3;
			Item.buffTime = 60 * 60 * 10;
			Item.rare = ItemRarityID.Expert;
		}
	}
}
