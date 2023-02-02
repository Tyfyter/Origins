using Origins.Items.Accessories;
using Origins.NPCs.Defiled;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Defiled_Amalgamation_Bag : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag (Defiled Amalgamation)");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRuleCondition master = new Conditions.IsMasterMode();
			itemLoot.Add(Defiled_Amalgamation.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Reshaping_Chunk>()));
			//itemLoot.Add(ItemDropRule.ByCondition(master, ModContent.ItemType<Mysterious_Spray>()));
			itemLoot.Add(ItemDropRule.Coins(Item.buyPrice(gold: 3), false));
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
