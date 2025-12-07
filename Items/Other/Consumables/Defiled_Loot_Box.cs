using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Defiled_Loot_Box : ModItem {
        public string[] Categories => [
            "GrabBag"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			//itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<A_Music_Box>(), 50));
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
		}
		public override bool CanRightClick() => true;
	}
}
