using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Pincushion : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Pincushion");
			// Tooltip.SetDefault("Prevents tile destruction from explosives");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().pincushion = true;
		}
	}
}
