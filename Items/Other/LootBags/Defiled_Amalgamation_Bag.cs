using Origins.Items.Accessories;
using Origins.NPCs.Defiled.Boss;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Defiled_Amalgamation_Bag : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.BossBag[Type] = true;
			ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(Defiled_Amalgamation.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Refactoring_Pieces>()));
			itemLoot.Add(ItemDropRule.Coins(Item.sellPrice(silver: 25), false));
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
