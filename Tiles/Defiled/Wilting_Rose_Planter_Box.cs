using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;

namespace Origins.Tiles.Defiled {
	//TODO: waiting on pull #4061 or similar addition
	public class Wilting_Rose_Planter_Box : Platform_Tile, ICustomWikiStat {
        public string[] Categories => [
            "PlanterBox"
        ];
		public override Color MapColor => new(44, 39, 58);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AdjTiles = [TileID.PlanterBox];
		}
	}
}
