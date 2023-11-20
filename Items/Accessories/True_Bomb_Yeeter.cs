using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class True_Bomb_Yeeter : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Explosive"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 20);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.5f;
		}
	}
}
