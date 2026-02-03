using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles.Dev {
	public class Structure_Void_Marker : ModTile {
		public static bool IsVoid(Tile tile) => tile.HasTile && TileLoader.GetTile(tile.TileType) is Structure_Void_Marker;
		public override void Load() {
			Mod.AddContent(new TileItem(this, true));
		}
		public sealed override bool Slope(int i, int j) => false;
		public sealed override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	}
}
