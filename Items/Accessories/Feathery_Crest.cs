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
			if (player.controlJump) {
				player.gravity = 0.15f;
			} else if (player.controlDown && player.velocity.Y != 0f) {
				player.gravity = 1.4f;
			}
		}
	}
}
