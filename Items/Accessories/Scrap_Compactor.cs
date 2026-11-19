using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Scrap_Compactor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(48, 36);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 2);
			Item.expert = true;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().scrapCompactor = true;
		}
	}
}
