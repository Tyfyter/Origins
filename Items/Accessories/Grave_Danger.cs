using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Grave_Danger : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Grave Danger");
			Tooltip.SetDefault(""); //{GraveDangerDesc} and {GraveDangerHardDesc} go in here
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 42);
			Item.defense = 5;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().graveDanger = true;
		}
	}
}
