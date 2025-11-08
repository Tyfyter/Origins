using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ThoriumMod.Items.Donate;

namespace Origins.Core.Structures {
	public class PaintTile : SerializableTileDescriptor {
		readonly TileDescriptor[] descriptors = new TileDescriptor[PaintID.IlluminantPaint];
		public override void Load() {
			AddSerializer(tile => tile.TileColor == PaintID.None ? null : $"PaintTile({PaintID.Search.GetName(tile.TileColor)})");
		}
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
		public override void Load() { }
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
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile) {
					if (Main.tileFrameImportant[tile.TileType]) return null;
					if (OriginsSets.Tiles.StructureSerializer_PlaceAtFrame[tile.TileType].HasValue) return null;
					return $"PlaceTile({TileID.Search.GetName(tile.TileType)})";
				}
				return null;
			});
		}
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
	public class PlaceFramedTile : PlaceTile {
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && Main.tileFrameImportant[tile.TileType]) {
					if (OriginsSets.Tiles.StructureSerializer_PlaceAtFrame[tile.TileType].HasValue) return null;
					return $"PlaceFramedTile({TileID.Search.GetName(tile.TileType)},{tile.TileFrameX},{tile.TileFrameY})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters) {
			CachedTileType type = new(parameters[0]);
			short frameX = short.Parse(parameters[1]);
			short frameY = short.Parse(parameters[2]);
			return new((_, _, i, j) => {
				Tile tile = Main.tile[i, j];
				tile.HasTile = true;
				tile.TileType = type;
				tile.TileFrameX = frameX;
				tile.TileFrameY = frameY;
			});
		}
	}
	public class PlaceSpecialTile : PlaceTile {
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && OriginsSets.Tiles.StructureSerializer_PlaceAtFrame[tile.TileType] is Point frame && frame.X == tile.TileFrameX && frame.Y == tile.TileFrameY) {
					int style = TileObjectData.GetTileStyle(tile);
					style -= style % TileObjectData.GetTileData(tile).RandomStyleRange;
					return $"PlaceSpecialTile({TileID.Search.GetName(tile.TileType)},{style})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters) {
			CachedTileType type = new(parameters[0]);
			int style = parameters.Length > 1 ? int.Parse(parameters[1]) : 0;
			int dir = parameters.Length > 2 ? int.Parse(parameters[2]) : 0;
			return new((_, _, i, j) => {
				TileObject.CanPlace(i, j, type, style, dir, out TileObject objectData);
				TileObject.Place(objectData);
				TileLoader.GetTile(type)?.PlaceInWorld(i, j, null);
			});
		}
	}
	public class PlaceWall : SerializableTileDescriptor {
		readonly ConcurrentDictionary<string, TileDescriptor> descriptors = [];
		public override void Load() {
			AddSerializer(tile => tile.WallType == WallID.None ? null : $"PlaceWall({WallID.Search.GetName(tile.WallType)})");
		}
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
			public ushort Value => id ??= (ushort)WallID.Search.GetId(name);
			public static implicit operator ushort(CachedWallType type) => type.Value;
		}
	}
	public class SlopeTile : SerializableTileDescriptor {
		readonly TileDescriptor[] descriptors = new TileDescriptor[(int)BlockType.SlopeUpRight + 1];
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && tile.BlockType != BlockType.Solid) {
					return $"SlopeTile({tile.BlockType})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters) {
			BlockType blockType = Enum.Parse<BlockType>(parameters[0]);
			return descriptors[(int)blockType] ??= new((_, _, i, j) => {
				Tile tile = Main.tile[i, j];
				tile.BlockType = blockType;
			});
		}
	}
	public class Void : SerializableTileDescriptor {
		readonly TileDescriptor descriptor = new(null, true);
		protected override TileDescriptor Create(string[] parameters) => descriptor;
	}
	public class Empty : SerializableTileDescriptor {
		readonly TileDescriptor descriptor = new(null);
		public override void Load() {
			AddSerializer(tile => tile.HasTile ? null : "Empty");
		}
		protected override TileDescriptor Create(string[] parameters) => descriptor;
	}
}
