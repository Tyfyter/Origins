using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Dawn {
	public class Genesis_Grass : OriginTile {
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
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(100, 100, 10));
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			bool half = Main.tile[i, j].IsHalfBlock;
			SlopeType slope = Main.tile[i, j].Slope;
			Main.tile[i, j].ResetToType(TileID.Dirt);
			WorldGen.SquareTileFrame(i, j);
			Main.tile[i, j].SetHalfBlock(half);
			Main.tile[i, j].SetSlope(slope);
			NetMessage.SendTileSquare(-1, i, j, 1);
		}
		public class Genesis_Grass_Seeds : ModItem {
			public override void SetStaticDefaults() {
				Item.ResearchUnlockCount = 25;
			}
			public override void SetDefaults() {
				Item.CloneDefaults(ItemID.GrassSeeds);
				Item.placeStyle = ModContent.TileType<Genesis_Grass>();
			}
			public override bool ConsumeItem(Player player) {
				Main.tile[Player.tileTargetX, Player.tileTargetY].TileType = (ushort)Item.placeStyle;
				return true;
			}
		}
	}
}
