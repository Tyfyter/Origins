using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Other {
    [AutoloadEquip(EquipType.Head)]
	public class Cranivore_Beanie : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cranivore Beanie");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
		}
	}
}
