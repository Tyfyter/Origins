using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Brine_Leafed_Clover : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brine-Leaf Clover");
			Tooltip.SetDefault("Increases the likelihood of favorable outcomes based on how many leaves it has");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 28);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().brineClover = true;
		}
	}
}
