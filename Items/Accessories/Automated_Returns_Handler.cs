using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Automated_Returns_Handler : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
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
			player.spikedBoots += 2;
		}
	}
}
