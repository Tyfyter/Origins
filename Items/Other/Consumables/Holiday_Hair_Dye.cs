using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Holiday_Hair_Dye : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Holiday Hair Dye");
			// Tooltip.SetDefault("Hair appearance changes depending on the current holiday");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Green;
		}
	}
}