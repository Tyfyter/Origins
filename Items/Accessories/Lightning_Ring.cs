using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Lightning_Ring : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 22);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().lightningRing = true;
		}
	}
}
