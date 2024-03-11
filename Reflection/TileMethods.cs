using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class TileMethods : ILoadable {
		public static FastFieldInfo<Tile, uint> TileId;
		public delegate void _KillTile_GetItemDrops(int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops = false);
		public static _KillTile_GetItemDrops KillTile_GetItemDrops;
		public void Load(Mod mod) {
			TileId = new("TileId", BindingFlags.NonPublic);
			KillTile_GetItemDrops = typeof(WorldGen).GetMethod(nameof(KillTile_GetItemDrops), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<_KillTile_GetItemDrops>();
		}
		public void Unload() {
			TileId = null;
			KillTile_GetItemDrops = null;
		}
	}
}