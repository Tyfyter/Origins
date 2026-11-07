using Origins.Walls;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles.Dev; 
public class Structure_Void_Marker : ModTile {
	public static bool IsVoid(Tile tile) => tile.HasTile && TileLoader.GetTile(tile.TileType) is Structure_Void_Marker;
	public override void Load() {
		Mod.AddContent(new TileItem(this, true));
	}
	public sealed override bool Slope(int i, int j) => false;
	public sealed override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
}
public class Structure_Empty_Wall_Marker : OriginsWall {
	public override WallVersion WallVersions => WallVersion.Safe;
	public override Color MapColor => Color.Transparent;
	public static bool IsVoid(Tile tile) => WallLoader.GetWall(tile.WallType) is Structure_Empty_Wall_Marker;
	public override bool WallFrame(int i, int j, bool randomizeFrame, ref int style, ref int frameNumber) {
		return false;
	}

}
