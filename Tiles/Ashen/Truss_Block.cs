using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Truss_Block : OriginTile, IAshenTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			AddMapEntry(FromHexRGB(0xBE9170));

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = DustID.Iron;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
				case (1, 0):
				case (2, 0):
				case (3, 0):

				case (1, 2):
				case (2, 2):
				case (3, 2):

				case (6, 0):
				case (7, 0):
				case (8, 0):
				case (6, 1):
				case (7, 1):
				case (8, 1):
				case (6, 2):
				case (7, 2):
				case (8, 2):

				case (6, 4):
				case (7, 4):
				case (8, 4):

				case (9, 0):
				case (9, 1):
				case (9, 2):

				case (12, 0):
				case (12, 1):
				case (12, 2):

				case (9, 3):
				case (10, 3):
				case (11, 3):

				case (0, 3):
				case (1, 3):
				case (2, 3):
				case (3, 3):
				case (4, 3):
				case (5, 3):
				break;

				default:
				height = -1600;
				break;
			}
		}
	}
}
