using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public interface IScrapCompactor {
		public float HealthThreshold(NPC target);
		public float? Damage(Player player, NPC target) => null;
	}
	public class Scrap_Compactor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs("40");
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(46, 36);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().scrapCompactor = true;
		}
	}
}
