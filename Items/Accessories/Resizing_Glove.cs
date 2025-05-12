using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Resizing_Glove : ModItem, ICustomWikiStat {
		public override string Texture => typeof(Gun_Glove).GetDefaultTMLName();
		public string[] Categories => [
			"Combat",
			"MeleeBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().resizingGlove = true;
			player.autoReuseGlove = true;
		}
	}
}
