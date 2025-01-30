using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Origins.TileExtenstions;
using static Origins.Tiles.Brine.Baryte.MergeType;

namespace Origins.Tiles.Brine {
	[LegacyName("Sulphur_Stone", "Dolomite")]
	public class Baryte : OriginTile {
        public string[] Categories => [
            "Stone"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			Origins.TileBlocksMinecartTracks[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			AddMapEntry(new Color(18, 73, 56));
			MinPick = 195;
			HitSound = SoundID.Tink;
			DustType = DustID.GreenMoss;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			static MergeMatcher Merge(MergeType Up, MergeType Down, MergeType Left, MergeType Right, MergeType? UpLeft = null, MergeType? UpRight = null, MergeType? DownLeft = null, MergeType? DownRight = null) {
				static int? Conv(MergeType? val) => val.HasValue ? (int)val.Value : null; 
				return new((int)Up, (int)Down, (int)Left, (int)Right, Conv(UpLeft), Conv(UpRight), Conv(DownLeft), Conv(DownRight));
			}
			Point right = new(1, 0);
			Point down = new(0, 1);
			DoFraming(i, j, resetFrame, map: [(Type, (int)ROCK), (TileID.Mud, (int)_MUD)],
				(Merge(ROCK, ROCK, NONE, ROCK), new Point(0, 0), down),
				(Merge(NONE, ROCK, ROCK, ROCK), new Point(1, 0), right),
				(Merge(ROCK, ROCK, ROCK, ROCK), new Point(1, 1), right),
				(Merge(ROCK, NONE, ROCK, ROCK), new Point(1, 2), right),
				(Merge(ROCK, ROCK, ROCK, NONE), new Point(4, 0), down),
				(Merge(ROCK, ROCK, NONE, NONE), new Point(5, 0), down),
				(Merge(NONE, ROCK, NONE, NONE), new Point(6, 0), right),
				(Merge(ROCK, ROCK, ROCK, ROCK, UpLeft: NONE, UpRight: NONE), new Point(6, 1), right),
				(Merge(ROCK, ROCK, ROCK, ROCK, DownLeft: NONE, DownRight: NONE), new Point(6, 2), right),
				(Merge(ROCK, NONE, NONE, NONE), new Point(6, 3), right),
				(Merge(NONE, NONE, NONE, ROCK), new Point(9, 0), down),
				(Merge(ROCK, ROCK, ROCK, ROCK, UpLeft: NONE, DownLeft: NONE), new Point(10, 0), down),
				(Merge(ROCK, ROCK, ROCK, ROCK, UpRight: NONE, DownRight: NONE), new Point(11, 0), down),
				(Merge(NONE, NONE, ROCK, NONE), new Point(12, 0), down),
				(Merge(NONE, _MUD, ROCK, ROCK), new Point(13, 0), right),
				(Merge(_MUD, NONE, ROCK, ROCK), new Point(13, 1), right),
				(Merge(ROCK, ROCK, NONE, _MUD), new Point(13, 2), right),
				(Merge(ROCK, ROCK, _MUD, NONE), new Point(13, 3), right),
				(Merge(NONE, ROCK, NONE, ROCK), new Point(0, 3), new Point(2, 0)),
				(Merge(NONE, ROCK, ROCK, NONE), new Point(1, 3), new Point(2, 0)),
				(Merge(ROCK, NONE, NONE, ROCK), new Point(0, 4), new Point(2, 0)),
				(Merge(ROCK, NONE, ROCK, NONE), new Point(1, 4), new Point(2, 0)),
				(Merge(NONE, NONE, NONE, NONE), new Point(9, 3), right),
				(Merge(NONE, NONE, ROCK, ROCK), new Point(6, 4), right),
				(Merge(ROCK, ROCK, ROCK, ROCK, DownRight: _MUD), new Point(0, 5), new Point(0, 2)),
				(Merge(ROCK, ROCK, ROCK, ROCK, DownLeft: _MUD), new Point(1, 5), new Point(0, 2)),
				(Merge(ROCK, ROCK, ROCK, ROCK, UpRight: _MUD), new Point(0, 6), new Point(0, 2)),
				(Merge(ROCK, ROCK, ROCK, ROCK, UpLeft: _MUD), new Point(1, 5), new Point(0, 2)),
				(Merge(_MUD, ROCK, _MUD, ROCK), new Point(2, 5), new Point(0, 2)),
				(Merge(_MUD, ROCK, ROCK, _MUD), new Point(3, 5), new Point(0, 2)),
				(Merge(ROCK, _MUD, _MUD, ROCK), new Point(2, 6), new Point(0, 2)),
				(Merge(ROCK, _MUD, ROCK, _MUD), new Point(3, 6), new Point(0, 2)),
				(Merge(ROCK, _MUD, NONE, ROCK), new Point(4, 5), down),
				(Merge(ROCK, _MUD, ROCK, NONE), new Point(5, 5), down),
				(Merge(_MUD, ROCK, NONE, ROCK), new Point(4, 8), down),
				(Merge(_MUD, ROCK, ROCK, NONE), new Point(5, 8), down),
				(Merge(NONE, _MUD, NONE, NONE), new Point(6, 5), down),
				(Merge(_MUD, NONE, NONE, NONE), new Point(6, 8), down),
				(Merge(ROCK, _MUD, NONE, NONE), new Point(7, 5), down),
				(Merge(_MUD, ROCK, NONE, NONE), new Point(7, 8), down),
				(Merge(ROCK, _MUD, ROCK, ROCK), new Point(8, 5), right),
				(Merge(_MUD, ROCK, ROCK, ROCK), new Point(8, 6), right),
				(Merge(ROCK, ROCK, ROCK, _MUD), new Point(8, 7), down),
				(Merge(ROCK, ROCK, _MUD, ROCK), new Point(9, 7), down),
				(Merge(_MUD, ROCK, _MUD, _MUD), new Point(11, 5), down),
				(Merge(_MUD, _MUD, _MUD, ROCK), new Point(12, 5), down),
				(Merge(ROCK, _MUD, _MUD, _MUD), new Point(11, 8), down),
				(Merge(_MUD, _MUD, ROCK, _MUD), new Point(12, 8), down),
				(Merge(ROCK, ROCK, _MUD, _MUD), new Point(10, 7), down),
				(Merge(_MUD, _MUD, ROCK, ROCK), new Point(8, 10), right),
				(Merge(NONE, ROCK, _MUD, ROCK), new Point(0, 11), right),
				(Merge(NONE, ROCK, ROCK, _MUD), new Point(3, 11), right),
				(Merge(ROCK, NONE, _MUD, ROCK), new Point(0, 12), right),
				(Merge(ROCK, NONE, ROCK, _MUD), new Point(3, 12), right),
				(Merge(_MUD, _MUD, _MUD, _MUD), new Point(6, 11), right),
				(Merge(NONE, NONE, _MUD, _MUD), new Point(9, 11), right),
				(Merge(_MUD, _MUD, NONE, NONE), new Point(6, 12), down),
				(Merge(NONE, NONE, _MUD, NONE), new Point(0, 13), right),
				(Merge(NONE, NONE, NONE, _MUD), new Point(3, 13), right),
				(Merge(NONE, NONE, _MUD, ROCK), new Point(0, 14), right),
				(Merge(NONE, NONE, ROCK, _MUD), new Point(3, 14), right),

				(Merge(NONE, ROCK, NONE, _MUD), new Point(0, 3), new Point(2, 0)),
				(Merge(NONE, ROCK, _MUD, NONE), new Point(1, 3), new Point(2, 0)),
				(Merge(ROCK, NONE, NONE, _MUD), new Point(0, 4), new Point(2, 0)),
				(Merge(ROCK, NONE, _MUD, NONE), new Point(1, 4), new Point(2, 0)),
				(Merge(NONE, _MUD, NONE, ROCK), new Point(0, 3), new Point(2, 0)),
				(Merge(NONE, _MUD, ROCK, NONE), new Point(1, 3), new Point(2, 0)),
				(Merge(_MUD, NONE, NONE, ROCK), new Point(0, 4), new Point(2, 0)),
				(Merge(_MUD, NONE, ROCK, NONE), new Point(1, 4), new Point(2, 0))
			);
			return false;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j + 1).HasTile) {
				if (TileObject.CanPlace(i, j + 1, WorldGen.genRand.NextBool(3) ? TileType<Brineglow>() : TileType<Underwater_Vine>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
		}
		internal enum MergeType {
			NONE = -1,
			ROCK = 1,
			_MUD = 2
		}
	}
	[LegacyName("Sulphur_Stone_Item", "Dolomite_Item")]
	public class Baryte_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Baryte>());
		}
	}
}
