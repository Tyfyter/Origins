using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Mana_Drive : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Mana Drive");
			// Tooltip.SetDefault("Mana regenerates faster with higher speeds");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ButterscotchRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().warpDrive -= true;
		}
	}
}
