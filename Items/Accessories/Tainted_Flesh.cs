using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Tainted_Flesh : ModItem {
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(5);
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 4);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (!originPlayer.taintedFlesh) originPlayer.tornStrengthBoost.Flat += 0.05f;
			originPlayer.taintedFlesh = true;
		}
	}
}
