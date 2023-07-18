using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Donor_Wristband : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Donor Wristband");
			// Tooltip.SetDefault("Increased life regeneration\nDebuff and healing potion cooldown durations reduced by 37.5%");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegen += 2;
			player.pStone = true;
			player.GetModPlayer<OriginPlayer>().donorWristband = true;
		}
	}
}
