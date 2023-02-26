using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Cirrhosis_Abhorrence : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cirrhosis' Abhorrence");
			Tooltip.SetDefault("5 of the closest enemies have their stats reduced whilst being set ablaze and bleeding");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().cirrHosis = true;
		}
	}
}
