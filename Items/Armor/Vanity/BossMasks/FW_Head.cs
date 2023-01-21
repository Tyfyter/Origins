using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.BossMasks {
    [AutoloadEquip(EquipType.Head)]
	public class FW_Head : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Weaver Head");
			Tooltip.SetDefault("'A fish bowl with extra steps'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.White;
		}
	}
}
