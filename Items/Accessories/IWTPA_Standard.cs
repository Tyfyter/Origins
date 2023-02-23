using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class IWTPA_Standard : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("I-WTPA Standard");
			Tooltip.SetDefault("Reduces explosive fuse time");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().iwtpaStandard = true;
		}
	}
}
