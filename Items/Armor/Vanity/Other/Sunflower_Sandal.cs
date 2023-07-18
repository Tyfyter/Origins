using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Other {
	[AutoloadEquip(EquipType.Legs)]
	public class Sunflower_Sandal : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Sunflower Sandals");
			// Tooltip.SetDefault("Quite fashionable");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Green;
		}
	}
}
