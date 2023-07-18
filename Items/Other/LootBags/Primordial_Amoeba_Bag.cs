using Origins.Items.Accessories;
using Origins.NPCs.Riven;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Primordial_Amoeba_Bag : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Treasure Bag (Primordial Amoeba)");
			// Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRuleCondition master = new Conditions.IsMasterMode();
			//itemLoot.Add(Primordial_Amoeba.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Amebic_Vial>()));
			//itemLoot.Add(ItemDropRule.ByCondition(master, ModContent.ItemType<Protozoa_Food>()));
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
