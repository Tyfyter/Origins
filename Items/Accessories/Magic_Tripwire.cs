using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Magic_Tripwire : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Magic Tripwire");
			Tooltip.SetDefault("Improved mine blast radius and detection proximity\n'Uses Bluetooth'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().magicTripwire = true;
		}
	}
}
