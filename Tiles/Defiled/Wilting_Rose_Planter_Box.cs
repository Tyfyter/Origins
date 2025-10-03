using Origins.Dev;
using Terraria.ID;

namespace Origins.Tiles.Defiled {
	//TODO: waiting on pull #4061 or similar addition
	public class Wilting_Rose_Planter_Box : Platform_Tile, ICustomWikiStat {
        public string[] Categories => [
            "PlanterBox"
        ];
		public override int BaseTileID => TileID.PlanterBox;
		public override Color MapColor => new(44, 39, 58);
	}
}
