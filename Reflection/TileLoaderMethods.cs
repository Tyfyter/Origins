using PegasusLib;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class TileLoaderMethods : ILoadable {
		public static FastStaticFieldInfo<Dictionary<(int, int), int>> tileTypeAndTileStyleToItemType;
		public static FastStaticFieldInfo<Dictionary<int, int>> wallTypeToItemType;
		public static FastStaticFieldInfo<IList<ModTile>> tiles;
		public static FastStaticFieldInfo<IList<ModWall>> walls;
		public void Load(Mod mod) {
			tileTypeAndTileStyleToItemType = new(typeof(TileLoader), nameof(tileTypeAndTileStyleToItemType), BindingFlags.NonPublic);
			wallTypeToItemType = new(typeof(WallLoader), nameof(wallTypeToItemType), BindingFlags.NonPublic);
			tiles = new(typeof(TileLoader), nameof(tiles), BindingFlags.NonPublic);
			walls = new(typeof(WallLoader), nameof(walls), BindingFlags.NonPublic);
		}
		public void Unload() {
			tileTypeAndTileStyleToItemType = null;
			wallTypeToItemType = null;
			tiles = null;
			walls = null;
		}
	}
}