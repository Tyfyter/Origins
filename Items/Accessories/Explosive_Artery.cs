using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Explosive_Artery : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Explosive Artery");
			Tooltip.SetDefault("Bleeding enemies have a chance to explode every second\n'Explosions happen near me 'cause I'm that cool!'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().explosiveArtery = true;
		}
	}
}
