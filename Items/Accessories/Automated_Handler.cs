using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Automated_Handler : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Automated Returns Handler");
			Tooltip.SetDefault("Allows the ability to climb walls and dash\nGreatly increased movement speed and return speed for grapple hooks and harpoon guns\nGives a chance to dodge attacks");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.hasMagiluminescence = true;
			player.GetModPlayer<OriginPlayer>().turboReel2 = true;

			player.blackBelt = true;
			player.dashType = 1;
			player.equippedAnyWallSpeedAcc = true;
		}
	}
}
