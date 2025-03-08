using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Venom_Fang : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ToxicSource"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.venomFang = true;
		}
	}
}
