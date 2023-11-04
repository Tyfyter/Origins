using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Invisible_Ink : ModItem {
		
		public override void SetDefaults() {
			Item.value = Item.sellPrice(silver: 15);
			Item.rare = ItemRarityID.White;
		}
	}
}