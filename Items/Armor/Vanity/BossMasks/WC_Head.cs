using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class FW_Head : ModItem {
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetDefaults() {
			Item.rare = ItemRarityID.White;
			Item.vanity = true;
		}
	}
}
