using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Limestone {
	public class Limestone : ComplexFrameTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			//TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.ChecksForMerge[Type] = true;
			// for some reason these hardened sand and sandstone aren't true in isDeserBiomeSand
			Main.tileMerge[Type][TileID.CorruptSandstone] = true;
			Main.tileMerge[TileID.CorruptSandstone][Type] = true;
			Main.tileMerge[Type][TileID.CorruptHardenedSand] = true;
			Main.tileMerge[TileID.CorruptHardenedSand][Type] = true;
			Main.tileMerge[Type][TileID.CrimsonSandstone] = true;
			Main.tileMerge[TileID.CrimsonSandstone][Type] = true;
			Main.tileMerge[Type][TileID.CrimsonHardenedSand] = true;
			Main.tileMerge[TileID.CrimsonHardenedSand][Type] = true;
			Main.tileMerge[Type][TileID.HallowSandstone] = true;
			Main.tileMerge[TileID.HallowSandstone][Type] = true;
			Main.tileMerge[Type][TileID.HallowHardenedSand] = true;
			Main.tileMerge[TileID.HallowHardenedSand][Type] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (Type != i && TileID.Sets.isDesertBiomeSand[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			AddMapEntry(new Color(180, 172, 134));
			DustType = DustID.Sand;
			HitSound = SoundID.Tink;
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Sand_Overlay", TileID.Sand);
			yield return new TileMergeOverlay(merge + "Ebonsand_Overlay", TileID.Ebonsand);
			yield return new TileMergeOverlay(merge + "Crimsand_Overlay", TileID.Crimsand);
			yield return new TileMergeOverlay(merge + "Pearlsand_Overlay", TileID.Pearlsand);
			yield return new TileMergeOverlay(merge + "Defiled_Sand_Overlay", TileType<Defiled_Sand>());
			yield return new TileMergeOverlay(merge + "Silica_Overlay", TileType<Silica>());
			yield return new TileMergeOverlay(merge + "Sootsand_Overlay", TileType<Sootsand>());
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
