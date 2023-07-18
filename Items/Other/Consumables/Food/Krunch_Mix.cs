using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Krunch_Mix : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Krunch Mix");
			// Tooltip.SetDefault("{$CommonItemTooltip.MinorStats} and further increased regeneration\n'Taste the graynbow'");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PotatoChips);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.4f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 10;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Regeneration, Item.buffTime);
			return true;
		}
	}
}
