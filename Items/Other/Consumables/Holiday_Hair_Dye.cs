using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Holiday_Hair_Dye : ModItem {
		
		public override void SetDefaults() {
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Green;
		}
	}
}