using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Tainted_Flesh : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tainted Flesh");
			Tooltip.SetDefault("Increases the strength of afflicted Torn effects");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 4);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().taintedFlesh = true;
		}
	}
}
