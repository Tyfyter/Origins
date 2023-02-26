using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Superstar_Celebration_Gear : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Superstar Celebration Gear");
			Tooltip.SetDefault("Improvements to all stats and stars will fall from the sky during a party\nAllows the holder to quadruple jump\n8% reduced mana cost\nAutomatically use mana potions when needed");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().superStar = true;
		}
	}
}
