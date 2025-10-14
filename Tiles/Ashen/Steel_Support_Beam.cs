using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
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
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.IsBeam[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
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
				tile.TileFrameY = 18 * 4;
				break;

				case (false, true):
				tile.TileFrameX = (short)(tile.TileFrameNumber * 18);
				tile.TileFrameY = 0;
				break;

				case (true, true):
				tile.TileFrameX = (short)(tile.TileFrameNumber * 18);
				tile.TileFrameY = (short)(18 * Main.rand.Next(1, 4));
				break;
			}
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY < 4 * 18 && tile.TileFrameX < 3 * 18) return;
			Lighting.GetCornerColors(i, j + 1, out VertexColors vertices);
			Vector2 pos = new Vector2(i * 16, (j + 1) * 16) - Main.screenPosition;
			if (!Main.drawToScreen) {
				pos.X += Main.offScreenRange;
				pos.Y += Main.offScreenRange;
			}
			Vector4 destination = new(pos, 16, 2);
			Rectangle source = new(tile.TileFrameX, tile.TileFrameY + 16, 16, 2);
			Main.tileBatch.Draw(
				TextureAssets.Tile[Type].Value,
				destination,
				source,
				vertices
			);
		}
	}
}
