using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Lousy_Liver : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lousy Liver");
			Tooltip.SetDefault("4 of the closest enemies have their stats reduced");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().lousyLiver = true;
		}
	}
}
