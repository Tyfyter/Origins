using Origins.Items.Accessories;
using Origins.NPCs.Defiled;
using Origins.NPCs.Fiberglass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Fiberglass_Weaver_Bag : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.CultistBossBag;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag (Fiberglass Weaver)");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRuleCondition master = new Conditions.IsMasterMode();
			itemLoot.Add(Fiberglass_Weaver.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Dagger>()));
			itemLoot.Add(ItemDropRule.ByCondition(master, ModContent.ItemType<Entangled_Energy>()));
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
