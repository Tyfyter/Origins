using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Feathery_Crest : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 85);
		}
		public override void UpdateEquip(Player player) {
			if (player.controlUp) {
				player.gravity -= 0.2f;
			} else if (player.controlDown) {
				player.gravity += 0.4f;
			}
		}
	}
}
