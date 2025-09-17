using Origins.Items.Accessories;
using Origins.NPCs.Brine.Boss;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Lost_Diver_Bag : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.BossBag[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(Mildew_Carrion.normalDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Retaliatory_Tendril>()));
			itemLoot.Add(ItemDropRule.Coins(Item.sellPrice(silver: 240), false));
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
