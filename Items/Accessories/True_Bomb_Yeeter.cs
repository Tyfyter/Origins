using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class True_Bomb_Yeeter : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Pneumatic Bomb Thrower");
			// Tooltip.SetDefault("50% increased explosive throwing velocity\nAlso commonly referred to as the 'True Bomb Yeeter'");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 20);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.5f;
		}
	}
}
