using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core;

[ReinitializeDuringResizeArrays]
public class RopeConnectThroughOverride : ILoadable {
	public static bool[] Set = TileID.Sets.Factory.CreateBoolSet();
	void ILoadable.Load(Mod mod) {
		On_WorldGen.IsRope += On_WorldGen_IsRope;
	}
	static bool On_WorldGen_IsRope(On_WorldGen.orig_IsRope orig, int x, int y) {
		if (orig(x, y)) return true;
		if (Set[Main.tile[x, y].TileType] && Main.tile[x, y - 1].HasTile && Main.tile[x, y + 1].HasTile && Main.tileRope[Main.tile[x, y - 1].TileType] && Main.tileRope[Main.tile[x, y + 1].TileType]) {
			return true;
		}
		return false;
	}
	void ILoadable.Unload() { }
}
