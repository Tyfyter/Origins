using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Trap_Charm : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Misc"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 24);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().trapCharm = true;
		}
	}
}
