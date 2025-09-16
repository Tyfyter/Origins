using Microsoft.Xna.Framework;
using Origins.Tiles.Cubekon;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Quartz : OriginTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileMerge[TileType<Silica>()][Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.HardenedSand[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			mergeID = TileID.HardenedSand;
			DustType = DustID.Ghost;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			TileExtenstions.DoFraming(i, j, resetFrame, map: [(Type, 1), (TileType<Brittle_Quartz>(), 2), (TileType<Silica>(), 2)], TileExtenstions.ExtraTileBlending);
			return false;
		}
	}
	public class Quartz_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Silica_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.HardenedSand, Type);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Quartz>());
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.DesertTorch, 3)
			.AddIngredient(ItemID.Torch, 3)
			.AddIngredient(Type)
			.SortAfterFirstRecipesOf(ItemID.DesertTorch)
			.Register();
		}
	}
}
