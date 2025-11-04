using Origins.Core;
using Origins.Dev;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
	public class Riven_Grass : ComplexFrameTile, IRivenTile {
        public override void SetStaticDefaults() {
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
			TileID.Sets.Conversion.MergesWithDirtInASpecialWay[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeDugByShovel[Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileBrick[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(0, 100, 160));
			DustType = Riven_Hive.DefaultTileDust;
			Riven_Grass_Seeds.TileAssociations[TileID.Dirt] = Type;
		}
		public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => tileTypeBeingPlaced != TileID.Dirt;
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Spug_Overlay", ModContent.TileType<Spug_Flesh>());
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (Main.rand.NextBool(250)) {
					above.SetToType((ushort)ModContent.TileType<Acetabularia>(), Main.tile[i, j].TileColor);
				} else {
					above.SetToType((ushort)ModContent.TileType<Riven_Foliage>(), Main.tile[i, j].TileColor);
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) {
				Framing.GetTileSafely(i, j).TileType = TileID.Dirt;
			}
		}
	}
	public class Riven_Jungle_Grass : ComplexFrameTile, IRivenTile {
		public override void SetStaticDefaults() {
			if (ModLoader.HasMod("InfectedQualities")) {
				TileID.Sets.JungleBiome[Type] = 1;
				TileID.Sets.RemixJungleBiome[Type] = 1;
			}
			TileID.Sets.GrassSpecial[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.JungleGrass[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeDugByShovel[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.JungleGrass];
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			Main.tileMerge[Type][TileID.LihzahrdBrick] = true;
			Main.tileMerge[TileID.LihzahrdBrick][Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(0, 100, 160));
			Riven_Grass_Seeds.TileAssociations[TileID.Mud] = Type;
		}
		public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => tileTypeBeingPlaced != TileID.Mud;
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) {
				Framing.GetTileSafely(i, j).TileType = TileID.Mud;
			}
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (Main.rand.NextBool(250)) {
					above.SetToType((ushort)ModContent.TileType<Acetabularia>(), Main.tile[i, j].TileColor);
				} else {
					above.SetToType((ushort)ModContent.TileType<Riven_Foliage>(), Main.tile[i, j].TileColor);
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Spug_Overlay", ModContent.TileType<Spug_Flesh>());
		}
	}
	[ReinitializeDuringResizeArrays]
	public class Riven_Grass_Seeds : ModItem, ICustomPlaceTileItem {
		public static int[] TileAssociations = TileID.Sets.Factory.CreateIntSet(-1);
		public override void SetStaticDefaults() {
			ItemID.Sets.GrassSeeds[Type] = true;
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimsonSeeds);
			Item.placeStyle = ModContent.TileType<Riven_Grass>();
		}
		public void PlaceTile(On_Player.orig_PlaceThing_Tiles orig, bool inRange) {
			if (inRange) CustomPlaceTileItem.PlantSeedsAtCursor(TileAssociations);
		}
	}
}
