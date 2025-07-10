using Origins.Items.Accessories;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Shimmer_Construct_Bag : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.BossBag[Type] = true;
			ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(Shimmer_Construct.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Shimmer_Shield>()));
			itemLoot.Add(ItemDropRule.Coins(Item.buyPrice(gold: 7), false));
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (Item.newAndShiny && !Item.shimmered) {
				Item.shimmered = true;
				Item.shimmerTime = 1;
			}
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
