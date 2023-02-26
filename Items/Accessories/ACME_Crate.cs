using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class ACME_Crate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("ACME Crate");
			Tooltip.SetDefault("Greatly increased explosive blast radius");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveBlastRadius *= 2.2f;
			player.GetModPlayer<OriginPlayer>().magicTripwire = true;
		}
	}
}
