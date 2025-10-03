using Origins.Items.Accessories;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Trenchmaker_Bag : ModItem {
		public override string Texture => typeof(Defiled_Amalgamation_Bag).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ItemID.Sets.BossBag[Type] = true;
			ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			//itemLoot.Add(Trenchmaker.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Stack_of_Shraps>()));
			itemLoot.Add(ItemDropRule.Coins(Item.sellPrice(silver: 25), false));
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
