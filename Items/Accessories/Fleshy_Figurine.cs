using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Fleshy_Figurine : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fleshy Figurine");
			Tooltip.SetDefault("Attacks tenderize targets\nIncreases the strength of afflicted Torn effects");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().taintedFlesh2 = true;
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
		}
	}
}
