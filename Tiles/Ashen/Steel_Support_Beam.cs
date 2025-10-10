using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Steel_Support_Beam : OriginTile, IAshenTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.Stone[Type] = false;
			TileID.Sets.Conversion.Stone[Type] = false;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			AddMapEntry(FromHexRGB(0x6b5e56));

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (resetFrame) tile.TileFrameNumber = Main.rand.Next(3);
			int y = j;
			for (; y > 0; y--) {
				if (!Framing.GetTileSafely(i, y - 1).TileIsType(Type)) break;
			}
			int frameNum = Framing.GetTileSafely(i, y).TileFrameNumber;
			for (; y < Main.maxTilesX; y++) {
				Tile part = Framing.GetTileSafely(i, y);
				if (!part.TileIsType(Type)) break;
				part.TileFrameNumber = frameNum;
				part.TileFrameX = (short)(frameNum * 18);
			}
			switch ((Framing.GetTileSafely(i, j - 1).TileIsType(Type), Framing.GetTileSafely(i, j + 1).TileIsType(Type))) {
				case (false, false):
				tile.TileFrameX = (short)((tile.TileFrameNumber + 3) * 18);
				tile.TileFrameY = 0;
				break;

				case (true, false):
				tile.TileFrameX = (short)(tile.TileFrameNumber * 18);
				tile.TileFrameY = 18 * 2;
				break;

				case (false, true):
				tile.TileFrameX = (short)(tile.TileFrameNumber * 18);
				tile.TileFrameY = 0;
				break;

				case (true, true):
				tile.TileFrameX = (short)(tile.TileFrameNumber * 18);
				tile.TileFrameY = 18;
				break;
			}
			return false;
		}
	}
}
