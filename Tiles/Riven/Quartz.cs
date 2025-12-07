using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Quartz : ComplexFrameTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileMerge[TileType<Silica>()][Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			mergeID = TileID.Sandstone;
			DustType = DustID.Ghost;
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Silica_Overlay", TileType<Silica>());
		}
	}
	public class Quartz_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<Silica_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.Sandstone, Type);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Quartz>());
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
