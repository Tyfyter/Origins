using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ID.ItemID;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Other.Fish {
    #region chunky crate
    public class Chunky_Crate : Fishing_Crate_Item<Chunky_Crate_Tile> {
		public override void ModifyItemLoot(ItemLoot itemLoot) {

			IItemDropRule[] riven = new IItemDropRule[4] {
				Defiled_Wastelands.FissureDropRule,
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Chunky_Crate_Tile : Fishing_Crate_Tile<Chunky_Crate> {
		public override string Texture => "Origins/Items/Other/Fish/Chunky_Crate";
		public override Color MapColor => new Color(200, 200, 200);
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Chunky_Crate>());
		}
	}
	#endregion
	#region bilious crate
	public class Bilious_Crate : Fishing_Crate_Item<Bilious_Crate_Tile> {
		public override void ModifyItemLoot(ItemLoot itemLoot) {

			IItemDropRule[] riven = new IItemDropRule[] {
				Defiled_Wastelands.FissureDropRule,
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores.Concat(HardmodeOres).ToArray()),
					new OneFromRulesRule(3, 2, Bars.Concat(HardmodeBars).ToArray())),
				new OneFromRulesRule(3, Potions),
				BiomeCrate_SoulOfNight,
				ItemDropRule.NotScalingWithLuck(ItemType<Black_Bile>(), 2, 2, 5)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Bilious_Crate_Tile : Fishing_Crate_Tile<Bilious_Crate> {
		public override Color MapColor => new Color(100, 100, 100);
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Chunky_Crate>());
		}
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
			IItemDropRule[] riven = new IItemDropRule[4] {
				Riven_Hive.LesionDropRule,
				BiomeChest_GoldCoin, 
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Crusty_Crate_Tile : Fishing_Crate_Tile<Crusty_Crate> {
		public override string Texture => "Origins/Items/Other/Fish/Crusty_Crate";
		public override Color MapColor => new Color(0, 125, 165);
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Crusty_Crate>());
		}
	}
	#endregion
	#region festering crate
	public class Festering_Crate : Fishing_Crate_Item<Festering_Crate_Tile> {
		public override void ModifyItemLoot(ItemLoot itemLoot) {

			IItemDropRule[] riven = new IItemDropRule[] {
				Riven_Hive.LesionDropRule,
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores.Concat(HardmodeOres).ToArray()),
					new OneFromRulesRule(3, 2, Bars.Concat(HardmodeBars).ToArray())),
				new OneFromRulesRule(3, Potions),
				BiomeCrate_SoulOfNight,
				ItemDropRule.NotScalingWithLuck(ItemType<Alkahest>(), 2, 2, 5)
			};
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	public class Festering_Crate_Tile : Fishing_Crate_Tile<Festering_Crate> {
		public override Color MapColor => new Color(100, 100, 100);
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Chunky_Crate>());
		}
	}
	#endregion
	public abstract class Fishing_Crate_Item<TTile> : ModItem where TTile : ModTile {
		public static IItemDropRule BiomeChest_GoldCoin => ItemDropRule.Common(GoldCoin, 4, 5, 13);//normally NotScalingWithLuck
		public static IItemDropRule BiomeCrate_SoulOfNight => ItemDropRule.NotScalingWithLuck(SoulofNight, 2, 2, 5);
		public static IItemDropRule[] Ores => new IItemDropRule[8] {
			ItemDropRule.NotScalingWithLuck(CopperOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(TinOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(IronOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(LeadOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(SilverOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(TungstenOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(GoldOre, 1, 30, 49),
			ItemDropRule.NotScalingWithLuck(PlatinumOre, 1, 30, 49)
		};
		public static IItemDropRule[] Bars => new IItemDropRule[6] {
			ItemDropRule.NotScalingWithLuck(IronBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(LeadBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(SilverBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(TungstenBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(GoldBar, 1, 10, 20),
			ItemDropRule.NotScalingWithLuck(PlatinumBar, 1, 10, 20)
		};
		public static IItemDropRule[] HardmodeOres => new IItemDropRule[6] {
			ItemDropRule.NotScalingWithLuck(364, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(1104, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(365, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(1105, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(366, 1, 20, 35),
			ItemDropRule.NotScalingWithLuck(1106, 1, 20, 35)
		};
		public static IItemDropRule[] HardmodeBars => new IItemDropRule[6] {
			ItemDropRule.NotScalingWithLuck(381, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(1184, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(382, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(1191, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(391, 1, 5, 16),
			ItemDropRule.NotScalingWithLuck(1198, 1, 5, 16)
		};
		public static IItemDropRule[] Potions => new IItemDropRule[6] {
			ItemDropRule.NotScalingWithLuck(ObsidianSkinPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(SpelunkerPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(HunterPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(GravitationPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(MiningPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(HeartreachPotion, 1, 2, 4)
		};
		public static IItemDropRule[] BiomeCrate_ExtraPotions => new IItemDropRule[2] {
			ItemDropRule.NotScalingWithLuck(188, 1, 5, 17),
			ItemDropRule.NotScalingWithLuck(189, 1, 5, 17)
		};
		public static IItemDropRule[] BiomeCrate_ExtraBait => new IItemDropRule[2] {
			ItemDropRule.NotScalingWithLuck(2676, 3, 2, 6),
			ItemDropRule.NotScalingWithLuck(2675, 1, 2, 6)
		};
		public virtual bool Hardmode => false;
		public override void SetStaticDefaults() {
			ItemID.Sets.IsFishingCrate[Type] = true;
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
			data.CoordinateHeights = new int[2] { 16, 18 };
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
