using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace Origins.Core.Structures {
	public class PaintTile : SerializableTileDescriptor {
		readonly TileDescriptor[] descriptors = new TileDescriptor[PaintID.IlluminantPaint];
		protected override TileDescriptor Create(string[] parameters) {
			if (!parameters[0].EndsWith("Paint")) parameters[0] += "Paint";
			byte paintType = (byte)PaintID.Search.GetId(parameters[0]);
			return descriptors[paintType] ??= new((_, _, i, j) => {
				Tile tile = Main.tile[i, j];
				tile.TileColor = paintType;
			});
		}
	}
	public class ConditionalTile : PlaceTile {
		protected override TileDescriptor Create(string[] parameters) {
			Accumulator<HashSet<char>, bool> condition = null;
			bool isInverted = false;
			for (int i = 0; i < parameters[0].Length; i++) {
				char socket = parameters[0][i];
				switch (socket) {
					case '!':
					isInverted ^= true;
					break;
					default:
					condition += (HashSet<char> connectedSockets, ref bool canGenerate) => {
						if (!canGenerate) return;
						canGenerate = connectedSockets.Contains(socket) == isInverted;
					};
					break;
				}
			}
			CachedTileType type = new(parameters[1]);
			return new((_, connectedSockets, i, j) => {
				if (condition.Accumulate(connectedSockets, true)) {
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = type;
				}
			});
		}
	}
	public class PlaceTile : SerializableTileDescriptor {
		readonly ConcurrentDictionary<string, TileDescriptor> descriptors = [];
		protected override TileDescriptor Create(string[] parameters) {
			return descriptors.GetOrAdd(parameters[0], parameter => {
				CachedTileType type = new(parameter);
				return new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = type;
				});
			});
		}
		public class CachedTileType(string name) {
			readonly string name = name;
			ushort? id;
			public ushort Value => id ??= (ushort)TileID.Search.GetId(name);
			public static implicit operator ushort(CachedTileType type) => type.Value;
		}
	}
	public class PlaceWall : SerializableTileDescriptor {
		readonly ConcurrentDictionary<string, TileDescriptor> descriptors = [];
		protected override TileDescriptor Create(string[] parameters) {
			return descriptors.GetOrAdd(parameters[0], parameter => {
				CachedWallType type = new(parameter);
				return new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.WallType = type;
				});
			});
		}
		public class CachedWallType(string name) {
			readonly string name = name;
			ushort? id;
			public ushort Value => id ??= (ushort)TileID.Search.GetId(name);
			public static implicit operator ushort(CachedWallType type) => type.Value;
		}
	}
	public class Void : SerializableTileDescriptor {
		readonly TileDescriptor descriptor = new(null, true);
		protected override TileDescriptor Create(string[] parameters) => descriptor;
	}
	public class Empty : SerializableTileDescriptor {
		readonly TileDescriptor descriptor = new(null);
		protected override TileDescriptor Create(string[] parameters) => descriptor;
	}
}
