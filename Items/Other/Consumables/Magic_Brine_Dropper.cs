using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Magic_Brine_Dropper : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Magic Brine Dropper");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(copper: 40);
			Item.rare = ItemRarityID.White;
		}
	}
}