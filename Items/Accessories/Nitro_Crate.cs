using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Nitro_Crate : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 1, silver: 60);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.2f;
		}
	}
}
