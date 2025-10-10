using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Hardened_Sootsand : OriginTile, IAshenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[TileType<Sootsand>()][Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.HardenedSand[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.HardenedSand];
            Main.tileMerge[TileID.HardenedSand][Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.HardenedSand];
            Main.tileMerge[Type][TileID.HardenedSand] = true;*/
			AddMapEntry(FromHexRGB(0x75678a));
			mergeID = TileID.HardenedSand;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			TileExtenstions.DoFraming(i, j, resetFrame, map: [(Type, 1), (TileType<Sootsand>(), 2)], TileExtenstions.ExtraTileBlending);
			return false;
		}
	}
	public class Hardened_Sootsand_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<Sootsand_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.HardenedSand, Type);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Hardened_Sootsand>());
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
