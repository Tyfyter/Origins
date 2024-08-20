using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Emergency_Bee_Canister : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"MasterAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 30);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().emergencyBeeCanister = true;
		}
	}
}
