using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles.Dev {
	public class Structure_Void_Marker : ModTile {
		public static bool IsVoid(Tile tile) => ModContent.GetInstance<Structure_Void_Marker>() is Structure_Void_Marker @void && tile.TileIsType(@void.Type);
		public override void Load() {
			Mod.AddContent(new TileItem(this, true));
		}
		public override bool Slope(int i, int j) => false;
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	}
}
