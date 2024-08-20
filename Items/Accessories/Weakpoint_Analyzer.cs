using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Weakpoint_Analyzer : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(14, 28);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().weakpointAnalyzer = true;
		}
	}
}
