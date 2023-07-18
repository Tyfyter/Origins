using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class Rope_Of_Sharing : ModItem {
		public static int WaistSlot { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Rope of Sharing");
			// Tooltip.SetDefault("All stats, afflictions and other effects are shared with another connected player");
			WaistSlot = Item.waistSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().sharingisCaring = true;
		}
	}
}
