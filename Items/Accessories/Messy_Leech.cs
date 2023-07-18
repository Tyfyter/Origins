using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Messy_Leech : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Messy Leech");
			// Tooltip.SetDefault("All attacks inflict Bleeding\nPrevents Defiled enemies from regenerating");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 26);
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.White;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().messyLeech = true;
		}
	}
}
