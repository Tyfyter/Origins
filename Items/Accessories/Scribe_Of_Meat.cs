using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Scribe_Of_Meat : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Scribe of the Meat God");
			Tooltip.SetDefault("Transform into a miniature wall of flesh during a dash");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 6);
			Item.master = true;
			Item.rare = ItemRarityID.Master;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().meatScribe = true;
		}
	}
}
