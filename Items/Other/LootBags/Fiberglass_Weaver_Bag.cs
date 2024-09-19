using Origins.Items.Accessories;
using Origins.Items.Armor.Fiberglass;
using Origins.NPCs.Fiberglass;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.LootBags {
	public class Fiberglass_Weaver_Bag : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.BossBag[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.FewFromOptionsNotScalingWithLuck(2, 1, ModContent.ItemType<Fiberglass_Helmet>(), ModContent.ItemType<Fiberglass_Body>(), ModContent.ItemType<Fiberglass_Legs>()));
			itemLoot.Add(Fiberglass_Weaver.weaponDropRule);
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Entangled_Energy>()));
			itemLoot.Add(ItemDropRule.Coins(Item.sellPrice(gold: 3), false));
		}
		public override bool CanRightClick() {
			return true;
		}
	}
}
