using Origins.Dev;
using Origins.Tiles.Defiled;
using Terraria.ID;

namespace Origins.Tiles.Ashen {
	//TODO: waiting on pull #4061 or similar addition
	public class Surveysprout_Planter_Box : Platform_Tile, ICustomWikiStat {
		public override string Texture => typeof(Wilting_Rose_Planter_Box).GetDefaultTMLName();
        public string[] Categories => [
            WikiCategories.PlanterBox
        ];
		public override int BaseTileID => TileID.PlanterBox;
		public override Color MapColor => new(44, 39, 58);
	}
}
