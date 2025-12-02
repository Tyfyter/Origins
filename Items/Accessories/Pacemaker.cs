using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Pacemaker : ModItem, ICustomWikiStat {
		public static int DisableRegenTime(Player player) => (int)Utils.Remap(player.statLife, 0, player.statLifeMax2, 2 * 60, 7 * 60);
		public static float RegenMult => 5f;
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) => player.OriginPlayer().pacemaker = true;
	}
}
