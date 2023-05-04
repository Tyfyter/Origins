using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Defiled_Loot_Box : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Loot Box");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
		}
		public override bool CanRightClick() => true;
	}
}
