using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.MultiTypeMultiTile;

namespace Origins.Tiles.Ashen {
	public class Incomplete_Standing_Refinery : OriginTile, IMultiTypeMultiTile {
		public static int ID { get; private set; }
		public static ShapeMap Shape => field = field || new ShapeMap(
			new() {
				['X'] = (ushort)ModContent.TileType<Incomplete_Standing_Refinery>(),
				['+'] = (ushort)ModContent.TileType<Truss_Block>()
			},
			" + XX+XX XXX     |     XXX XX+XX + ",
			" + XX+XXXXXX     |     XXXXXX+XX + ",
			" +  X+XXX+XX     |     XX+XXX+X  + ",
			"+++++++++++++X   |   X+++++++++++++",
			" +   +XXX+XXXXXX | XXXXXX+XXX+   + ",
			" +XXX+XXX+XXXXXXX|XXXXXXX+XXX+XXX+ ",
			" +XXX+XXX+XXX+XXX|XXX+XXX+XXX+XXX+ ",
			" + XX+XXX+XXX+XX | XX+XXX+XXX+XX + ",
			"+++++++++++++++  |  +++++++++++++++",
			" +XXX+XXX+XXXX   |   XXXX+XXX+XXX+ ",
			" +XXX+XXX+XXXX   |   XXXX+XXX+XXX+ ",
			" +XXX+XXX+XXXX   |   XXXX+XXX+XXX+ ",
			" +XXX+XXX+XXXXX  |  XXXXX+XXX+XXX+ ",
			"++++++++++++++++ | ++++++++++++++++",
			" +XXX+XXX+XXXXX  |  XXXXX+XXX+XXX+ ",
			"  XXXXXXXXXXXXX  |  XXXXXXXXXXXXX  ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXXXX|XXXXXXXXXXXXXXXXX"
		);
		public override void Load() => new TileItem(this, true).RegisterItem();
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			// Names
			AddMapEntry(new Color(154, 56, 11));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 17;
			TileObjectData.newTile.SetHeight(29);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.HookPlaceOverride = Shape.Place;
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);
			ID = Type;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public bool IsValidTile(Tile tile, int left, int top, int style) => Shape.Matches(tile, left, top, style);
		public bool ShouldBlockPlacement(Tile tile, int left, int top, int style) {
			Point pos = tile.GetTilePosition();
			if (!Shape[pos.X - left, pos.Y - top, style]) return false;
			return MultiTypeMultiTile.NormallyBlocksPlacement(tile);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => Shape[x - left, y - top, style];
	}
}
