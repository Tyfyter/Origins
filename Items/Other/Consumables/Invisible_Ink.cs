using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Invisible_Ink : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Invisible Ink");
			Tooltip.SetDefault("Allows you to draw with ink that can only be seen under a black light");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(silver: 15);
			Item.rare = ItemRarityID.White;
		}
	}
}