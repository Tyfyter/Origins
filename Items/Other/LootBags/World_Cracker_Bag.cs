using Origins.Items.Accessories;
using Origins.NPCs.Riven.World_Cracker;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class World_Cracker_Bag : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(World_Cracker_Head.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Protozoa_Food>()));
			itemLoot.Add(ItemDropRule.Coins(Item.sellPrice(gold: 3), false));
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
