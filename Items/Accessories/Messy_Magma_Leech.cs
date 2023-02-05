using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Messy_Magma_Leech : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Messy Magma Leech");
			Tooltip.SetDefault("Melee attacks inflict Bleeding and set enemies ablaze\nPrevents Defiled enemies from regenerating");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 26);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().messyMagmaLeech = true;
		}
	}
}
