using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Retaliatory_Tendril : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 28);
			Item.useAnimation = 50; // trigger threshold
			Item.useTime = 1000; // "charge"
			Item.knockBack = 0.02f; // damage to strength multiplier
			Item.reuseDelay = 200; // max bonus, in percent
			Item.rare = ItemRarityID.Pink;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.retaliatoryTendril = true;
			originPlayer.retaliatoryTendrilItem = Item;
		}
	}
}
