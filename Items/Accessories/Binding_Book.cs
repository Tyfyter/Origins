using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Binding_Book : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",

		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
			player.GetModPlayer<OriginPlayer>().reshapingChunk = true;
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<OriginPlayer>().bindingBookVisual = true;
		}
	}
}
