using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
	public class Riven_Grass : ComplexFrameTile, IRivenTile {
        public string[] Categories => [
            "Grass"
        ];
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
		}
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
		}
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
	public class Riven_Grass_Seeds : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GrassSeeds[Type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimsonSeeds);
			Item.placeStyle = ModContent.TileType<Riven_Grass>();
		}
		public override bool ConsumeItem(Player player) {
			ref ushort tileType = ref Main.tile[Player.tileTargetX, Player.tileTargetY].TileType;
			switch (tileType) {
				case TileID.CrimsonGrass:
				tileType = (ushort)ModContent.TileType<Riven_Grass>();
				break;
				case TileID.CrimsonJungleGrass:
				tileType = (ushort)ModContent.TileType<Riven_Jungle_Grass>();
				break;
				default:
				return false;
			}
			WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY);
			if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY, tileType, 0);
			return true;
		}
	}
}
