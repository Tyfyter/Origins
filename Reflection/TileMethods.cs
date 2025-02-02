﻿using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using PegasusLib;

namespace Origins.Reflection {
	public class TileMethods : ILoadable {
		public static FastFieldInfo<Tile, uint> TileId;
		public delegate void _KillTile_GetItemDrops(int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops = false);
		public static _KillTile_GetItemDrops KillTile_GetItemDrops;
		public delegate void WorldGen_SpawnThingsFromPot_Del(int i, int j, int x2, int y2, int style);
		public static WorldGen_SpawnThingsFromPot_Del WorldGen_SpawnThingsFromPot { get; private set; }
		public void Load(Mod mod) {
			TileId = new("TileId", BindingFlags.NonPublic);
			KillTile_GetItemDrops = typeof(WorldGen).GetMethod(nameof(KillTile_GetItemDrops), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<_KillTile_GetItemDrops>();
			WorldGen_SpawnThingsFromPot = typeof(WorldGen).GetMethod("SpawnThingsFromPot", BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<WorldGen_SpawnThingsFromPot_Del>();
		}
		public void Unload() {
			TileId = null;
			KillTile_GetItemDrops = null;
			WorldGen_SpawnThingsFromPot = null;
		}
	}
}