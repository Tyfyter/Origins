using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class TileMethods : ILoadable {
		public static FastFieldInfo<Tile, uint> TileId;
		public void Load(Mod mod) {
			TileId = new("TileId", BindingFlags.NonPublic);
		}
		public void Unload() {
			TileId = null;
		}
	}
}