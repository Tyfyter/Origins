using Microsoft.Xna.Framework;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
	public class Riven_Grass : OriginTile, IRivenTile {
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
			//SetModTree(Defiled_Tree.Instance);
			DustType = Riven_Hive.DefaultTileDust;
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (Main.rand.NextBool(250)) {
					above.ResetToType((ushort)ModContent.TileType<Acetabularia>());
				} else {
					above.ResetToType((ushort)ModContent.TileType<Riven_Foliage>());
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
	public class Riven_Jungle_Grass : OriginTile, IRivenTile {
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
			Main.tileMerge[Type] = Main.tileMerge[TileID.JungleGrass];
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
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
			//SetModTree(Defiled_Tree.Instance);
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
					above.ResetToType((ushort)ModContent.TileType<Acetabularia>());
				} else {
					above.ResetToType((ushort)ModContent.TileType<Riven_Foliage>());
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Riven_Grass_Seeds : ModItem {
		public override void SetStaticDefaults() {
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
			}
			return true;
		}
	}
}
