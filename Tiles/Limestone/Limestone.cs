using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Limestone {
	public class Limestone : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			//TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.ChecksForMerge[Type] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (Main.tileMerge[i][TileID.Sand] || Main.tileMerge[i][TileID.HardenedSand] || Main.tileMerge[i][TileID.Sandstone]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			AddMapEntry(new Color(180, 172, 134));
			DustType = DustID.Sand;
			HitSound = SoundID.Tink;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			TileExtenstions.DoFraming(i, j, resetFrame, map: [(Type, 1), (TileID.Sand, 2)], TileExtenstions.ExtraTileBlending);
			return false;
		}
	}
	public class Limestone_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.Sandstone, Type);
			OriginExtensions.InsertIntoShimmerCycle(Type, ItemID.Granite);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Limestone>());
		}
    }
}
