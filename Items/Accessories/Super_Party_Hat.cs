using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Super_Party_Hat : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Super Party Hat");
			Tooltip.SetDefault("Greatly increased speed during parties\n'It's not just a party hat, it's a SUPER party hat!'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().superHat = true;
		}
	}
}
