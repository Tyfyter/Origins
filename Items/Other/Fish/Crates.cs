using Origins.World.BiomeData;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Crusty_Crate : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crusty Crate");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}\n'No, this is Patrick.'");
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.IsFishingCrate[Type] = true;
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimsonFishingCrate);
			Item.createTile = -1;
			Item.glowMask = glowmask;
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
		}
		public override bool CanRightClick() {
			return true;
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			var oresTier1 = new IItemDropRule[8] {
				ItemDropRule.NotScalingWithLuck(12, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(699, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(11, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(700, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(14, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(701, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(13, 1, 30, 49),
				ItemDropRule.NotScalingWithLuck(702, 1, 30, 49)
			};
			var barsTier1 = new IItemDropRule[6] {
				ItemDropRule.NotScalingWithLuck(22, 1, 10, 20),
				ItemDropRule.NotScalingWithLuck(704, 1, 10, 20),
				ItemDropRule.NotScalingWithLuck(21, 1, 10, 20),
				ItemDropRule.NotScalingWithLuck(705, 1, 10, 20),
				ItemDropRule.NotScalingWithLuck(19, 1, 10, 20),
				ItemDropRule.NotScalingWithLuck(706, 1, 10, 20)
			};
			var potions = new IItemDropRule[6] {
				ItemDropRule.NotScalingWithLuck(288, 1, 2, 4),
				ItemDropRule.NotScalingWithLuck(296, 1, 2, 4),
				ItemDropRule.NotScalingWithLuck(304, 1, 2, 4),
				ItemDropRule.NotScalingWithLuck(305, 1, 2, 4),
				ItemDropRule.NotScalingWithLuck(2322, 1, 2, 4),
				ItemDropRule.NotScalingWithLuck(2323, 1, 2, 4)
			};

			IItemDropRule[] riven = new IItemDropRule[4] {
				Riven_Hive.LesionDropRule, // bc_crimson change to match riven lesions
				ItemDropRule.Common(73, 4, 5, 13), //bc_goldCoin, normally NotScalingWithLuck
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, oresTier1),
					new OneFromRulesRule(3, 2, barsTier1)),
				new OneFromRulesRule(3, potions)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
		}
	}
}
