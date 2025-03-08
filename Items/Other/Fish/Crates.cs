using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ID.ItemID;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Other.Fish {
	#region chunky crate
	public class Chunky_Crate : Fishing_Crate_Item<Chunky_Crate_Tile> {
		public override void ModifyItemLoot(ItemLoot itemLoot) {

			IItemDropRule[] riven = [
				Defiled_Wastelands.FissureDropRule,
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Chunky_Crate_Tile : Fishing_Crate_Tile<Chunky_Crate> {
		public override string Texture => "Origins/Items/Other/Fish/Chunky_Crate";
		public override Color MapColor => new Color(200, 200, 200);
	}
	#endregion
	#region bilious crate
	public class Bilious_Crate : Fishing_Crate_Item<Bilious_Crate_Tile> {
		public override bool Hardmode => true;
		public override void ModifyItemLoot(ItemLoot itemLoot) {

			IItemDropRule[] riven = [
				Defiled_Wastelands.FissureDropRule,
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores.Concat(HardmodeOres).ToArray()),
					new OneFromRulesRule(3, 2, Bars.Concat(HardmodeBars).ToArray())),
				new OneFromRulesRule(3, Potions),
				BiomeCrate_SoulOfNight,
				ItemDropRule.NotScalingWithLuck(ItemType<Black_Bile>(), 2, 2, 5)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Bilious_Crate_Tile : Fishing_Crate_Tile<Bilious_Crate> {
		public override Color MapColor => new Color(100, 100, 100);
	}
	#endregion
	#region crusty crate
	public class Crusty_Crate : Fishing_Crate_Item<Crusty_Crate_Tile> {
		static short glowmask;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.glowMask = glowmask;
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] riven = [
				Riven_Hive.LesionDropRule,
				BiomeChest_GoldCoin, 
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Crusty_Crate_Tile : Fishing_Crate_Tile<Crusty_Crate> {
		public override string Texture => "Origins/Items/Other/Fish/Crusty_Crate";
		public override Color MapColor => new Color(0, 125, 165);
	}
	#endregion
	#region festering crate
	public class Festering_Crate : Fishing_Crate_Item<Festering_Crate_Tile> {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Origins.AddGlowMask(this);
		}
		public override bool Hardmode => true;
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] riven = [
				Riven_Hive.LesionDropRule,
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores.Concat(HardmodeOres).ToArray()),
					new OneFromRulesRule(3, 2, Bars.Concat(HardmodeBars).ToArray())),
				new OneFromRulesRule(3, Potions),
				BiomeCrate_SoulOfNight,
				ItemDropRule.NotScalingWithLuck(ItemType<Alkahest>(), 2, 2, 5)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Festering_Crate_Tile : Fishing_Crate_Tile<Festering_Crate> {
		public override Color MapColor => new Color(100, 100, 100);
	}
	#endregion
	#region basic crate
	public class Basic_Crate : Fishing_Crate_Item<Basic_Crate_Tile> {
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] brine = [
				//Riven_Hive.LesionDropRule,
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Brineglow_Item>(), 1, 5, 16),
					ItemDropRule.NotScalingWithLuck(ItemType<Peat_Moss_Item>(), 1, 5, 16)
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores.Concat(HardmodeOres).ToArray()),
					new OneFromRulesRule(3, 2, Bars.Concat(HardmodeBars).ToArray())),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(brine));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(new OneFromRulesRule(2,
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Focus_Potion>(), 1, 1, 3),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Antisolve_Potion>(), 1, 1, 3)
			));
			itemLoot.Add(ItemDropRule.OneFromOptions(4,
				ModContent.ItemType<Sour_Apple>(),
				ModContent.ItemType<Caeser_Salad>()
			));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Basic_Crate_Tile : Fishing_Crate_Tile<Basic_Crate> {
		public override string Texture => "Origins/Items/Other/Fish/Basic_Crate";
		public override Color MapColor => new Color(0, 62, 64);
	}
	#endregion
	public abstract class Fishing_Crate_Item<TTile> : ModItem, ICustomWikiStat where TTile : ModTile {
		string[] ICustomWikiStat.Categories {
			get {
				return [
					"GrabBag",
					..(Hardmode ? ["Hardmode"] : Array.Empty<string>()),
					..Categories
				];
			}
		}
		public virtual IEnumerable<string> Categories => [];
		public static IItemDropRule BiomeChest_GoldCoin => ItemDropRule.Common(GoldCoin, 4, 5, 13);//normally NotScalingWithLuck
		public static IItemDropRule BiomeCrate_SoulOfNight => ItemDropRule.NotScalingWithLuck(SoulofNight, 2, 2, 5);
		public static IItemDropRule[] Ores => [
			ItemDropRule.NotScalingWithLuck(CopperOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(TinOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(IronOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(LeadOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(SilverOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(TungstenOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(GoldOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(PlatinumOre, 1, 30, 49)
		];
		public static IItemDropRule[] Bars => [
			ItemDropRule.NotScalingWithLuck(IronBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(LeadBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(SilverBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(TungstenBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(GoldBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(PlatinumBar, 1, 10, 20)
		];
		public static IItemDropRule[] HardmodeOres => [
			ItemDropRule.NotScalingWithLuck(364, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(1104, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(365, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(1105, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(366, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(1106, 1, 20, 35)
		];
		public static IItemDropRule[] HardmodeBars => [
			ItemDropRule.NotScalingWithLuck(381, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(1184, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(382, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(1191, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(391, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(1198, 1, 5, 16)
		];
		public static IItemDropRule[] Potions => [
			ItemDropRule.NotScalingWithLuck(ObsidianSkinPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(SpelunkerPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(HunterPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(GravitationPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(MiningPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(HeartreachPotion, 1, 2, 4)
		];
		public static IItemDropRule[] BiomeCrate_ExtraPotions => [
			ItemDropRule.NotScalingWithLuck(188, 1, 5, 17),
			ItemDropRule.NotScalingWithLuck(189, 1, 5, 17)
		];
		public static IItemDropRule[] BiomeCrate_ExtraBait => [
			ItemDropRule.NotScalingWithLuck(2676, 3, 2, 6),
			ItemDropRule.NotScalingWithLuck(2675, 1, 2, 6)
		];
		public virtual bool Hardmode => false;
		public override void SetStaticDefaults() {
			ItemID.Sets.IsFishingCrate[Type] = true;
			ItemID.Sets.IsFishingCrateHardmode[Type] = Hardmode;
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimsonFishingCrate);
			Item.createTile = ModContent.TileType<TTile>();
			Item.placeStyle = 0;
			Item.rare = Hardmode ? ItemRarityID.LightRed : ItemRarityID.Green;
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
		}
		public override bool CanRightClick() {
			return true;
		}
	}
	public abstract class Fishing_Crate_Tile<TItem> : ModTile where TItem : ModItem, new() {
		public override string Texture => new TItem().Texture;
		public abstract Color MapColor { get; }
		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileTable[Type] = true;

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			SetTileObjectData(TileObjectData.newTile);
			TileObjectData.addTile(Type);

			SetMapEntry();
		}
		protected virtual void SetTileObjectData(TileObjectData data) {
			data.CoordinateHeights = [16, 18];
			data.StyleHorizontal = true; // Optional, if you add more placeStyles for the item
			data.CoordinatePadding = 0;
			data.CoordinateWidth = 16;
		}
		public virtual void SetMapEntry() {
			AddMapEntry(MapColor, CreateMapEntryName());
		}
		public override bool CreateDust(int i, int j, ref int type) => false;
	}
}
