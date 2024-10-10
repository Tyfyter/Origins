using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Fiberglass_Dagger : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.rare = ItemRarityID.Blue;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.statDefense -= 8;
			player.GetDamage(DamageClass.Default).Flat += 8;
			player.GetDamage(DamageClass.Generic).Flat += 8;
		}
	}
}
