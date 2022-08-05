using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Items.Other.Fish {
	#region chunky crate
	public class Chunky_Crate : Fishing_Crate_Item {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chunky Crate");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}\n'No, this is Patrick.'");
			ItemID.Sets.IsFishingCrate[Type] = true;
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimsonFishingCrate);
			Item.createTile = ModContent.TileType<Chunky_Crate_Tile>();
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
				Defiled_Wastelands.FissureDropRule,
				ItemDropRule.NotScalingWithLuck(73, 4, 5, 13), //bc_goldCoin, normally NotScalingWithLuck
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, oresTier1),
					new OneFromRulesRule(3, 2, barsTier1)),
				new OneFromRulesRule(3, potions)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
		}
	}
	public class Chunky_Crate_Tile : Fishing_Crate_Tile {
		public override string Texture => "Origins/Items/Other/Fish/Chunky_Crate";
		public override void SetMapEntry() {
			ModTranslation name = CreateMapEntryName("Origins.Items.Other.Fish.Chunky_Crate");
			name.SetDefault("Crate");
			AddMapEntry(new Color(200, 200, 200), name);
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Chunky_Crate>());
		}
	}
	#endregion
	#region crusty crate
	public class Crusty_Crate : Fishing_Crate_Item {
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
			Item.createTile = ModContent.TileType<Crusty_Crate_Tile>();
			Item.glowMask = glowmask;
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
				Riven_Hive.LesionDropRule,
				ItemDropRule.Common(73, 4, 5, 13), //bc_goldCoin, normally NotScalingWithLuck
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, oresTier1),
					new OneFromRulesRule(3, 2, barsTier1)),
				new OneFromRulesRule(3, potions)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
		}
	}
	public class Crusty_Crate_Tile : Fishing_Crate_Tile {
		public override string Texture => "Origins/Items/Other/Fish/Crusty_Crate";
		public override void SetMapEntry() {
			ModTranslation name = CreateMapEntryName("Origins.Items.Other.Fish.Crusty_Crate");
			name.SetDefault("Crate");
			AddMapEntry(new Color(0, 125, 165), name);
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Crusty_Crate>());
		}
	}
	#endregion
	public abstract class Fishing_Crate_Item : ModItem {
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
		}
		public override bool CanRightClick() {
			return true;
		}
	}
	public abstract class Fishing_Crate_Tile : ModTile {
		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileTable[Type] = true;

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
			TileObjectData.newTile.StyleHorizontal = true; // Optional, if you add more placeStyles for the item
			TileObjectData.newTile.CoordinatePadding = 0;
			TileObjectData.addTile(Type);

			SetMapEntry();
		}
		public abstract void SetMapEntry();
		public override bool CreateDust(int i, int j, ref int type) {
			return false;
		}
	}
}
