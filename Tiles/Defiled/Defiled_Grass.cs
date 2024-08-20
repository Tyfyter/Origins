using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Defiled {
	public class Defiled_Grass : OriginTile, IDefiledTile {
        public string[] Categories => [
            "Grass"
        ];
        public override void SetStaticDefaults() {
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Grass];
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			HitSound = Origins.Sounds.DefiledIdle;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			//SetModTree(Defiled_Tree.Instance);
			AddDefiledTile();
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			fail = true;
			noItem = true;
			OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
			if (!(originWorld is null)) {
				if (originWorld.defiledAltResurgenceTiles is null) originWorld.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
				originWorld.defiledAltResurgenceTiles.Add((i, j, Type));
			}
			bool half = Main.tile[i, j].IsHalfBlock;
			SlopeType slope = Main.tile[i, j].Slope;
			Main.tile[i, j].ResetToType(TileID.Dirt);
			WorldGen.SquareTileFrame(i, j);
			Main.tile[i, j].SetHalfBlock(half);
			Main.tile[i, j].SetSlope(slope);
			NetMessage.SendTileSquare(-1, i, j, 1);

		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid && Main.rand.NextBool(250)) {
				above.ResetToType((ushort)ModContent.TileType<Soulspore>());
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Defiled_Jungle_Grass : OriginTile, IDefiledTile {
		public override void SetStaticDefaults() {
			TileID.Sets.GrassSpecial[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.JungleGrass[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.JungleGrass];
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			HitSound = Origins.Sounds.DefiledIdle;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(180, 180, 180));
			//SetModTree(Defiled_Tree.Instance);
			AddDefiledTile();
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			fail = true;
			noItem = true;
			OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
			if (!(originWorld is null)) {
				if (originWorld.defiledAltResurgenceTiles is null) originWorld.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
				originWorld.defiledAltResurgenceTiles.Add((i, j, Type));
			}
			bool half = Main.tile[i, j].IsHalfBlock;
			SlopeType slope = Main.tile[i, j].Slope;
			Main.tile[i, j].ResetToType(TileID.Mud);
			WorldGen.SquareTileFrame(i, j);
			Main.tile[i, j].SetHalfBlock(half);
			Main.tile[i, j].SetSlope(slope);
			NetMessage.SendTileSquare(-1, i, j, 1);

		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid && Main.rand.NextBool(250)) {
				above.ResetToType((ushort)ModContent.TileType<Soulspore>());
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Defiled_Grass_Seeds : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GrassSeeds);
			Item.placeStyle = ModContent.TileType<Defiled_Grass>();
		}
		public override bool ConsumeItem(Player player) {
			Main.tile[Player.tileTargetX, Player.tileTargetY].TileType = (ushort)Item.placeStyle;
			return true;
		}
	}
}
