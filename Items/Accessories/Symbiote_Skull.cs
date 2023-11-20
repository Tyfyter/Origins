using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Symbiote_Skull : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Torn",
			"TornSource"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 28);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
		}
	}
}
