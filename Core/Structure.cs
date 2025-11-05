using Microsoft.Xna.Framework.Input;
using PegasusLib;
using PegasusLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static Origins.Tiles.ComplexFrameTile;

namespace Origins.Core {
	public abstract class Structure : ILoadable {
		protected List<IRoom> rooms = [];
		public virtual bool IsValidLayout() => true;
		public void Generate(int i, int j) {
			Dictionary<string, List<(IRoom room, char entrance)>[]> lookup = rooms.GenerateConnectionLookup();
			WeightedRandom<IRoom> start = new(WorldGen.genRand);
			for (int k = 0; k < rooms.Count; k++) {
				if (rooms[k].ValidStart) start.Add(rooms[k]);
			}
			IRoom startRoom = start.Get();
			List<RoomInstance> roomInstances = [
				new(startRoom, new Point(i, j) - startRoom.GetOrigin('C'), ['C'])
			];
			WeightedRandom<RoomInstance> roomsToExtend = new();
			roomsToExtend.Add(roomInstances[0]);
			WeightedRandom<(IRoom room, char entrance)> newRoom = new(WorldGen.genRand);

			int[] tileCount = new int[Main.maxTilesX * Main.maxTilesY];
			int[] tileCountSwap = new int[Main.maxTilesX * Main.maxTilesY];
			while (roomInstances.Count < 25 && roomsToExtend.TryPop(out RoomInstance nextRoom)) {
				foreach (KeyValuePair<char, RoomSocket> socket in nextRoom.Room.SocketKey) {
					if (socket.Value.Direction == Direction.None) continue;
					if (nextRoom.ConsumedConnections.Contains(socket.Key)) continue;
					if (!nextRoom.Room.Map.Contains(socket.Key)) continue;

					Direction direction = socket.Value.Direction;
					if (direction is Direction.Horizontal or Direction.Vertical) direction = nextRoom.Repetitions.Direction;
					Point pos = nextRoom.Position + nextRoom.Room.GetOrigin(socket.Key);
					switch (direction) {
						case Direction.Right:
						pos.X++;
						break;
						case Direction.Left:
						pos.X--;
						break;
						case Direction.Up:
						pos.Y--;
						break;
						case Direction.Down:
						pos.Y++;
						break;
					}

					newRoom.Clear();
					foreach ((IRoom room, char entrance) item in lookup[socket.Value.Key][(int)direction]) {
						newRoom.Add(item);
					}
					while (newRoom.TryPop(out (IRoom room, char entrance) room)) {
						RoomInstance newInstance = new(room.room, pos - room.room.GetOrigin(room.entrance), [room.entrance]);
						tileCount.CopyTo(tileCountSwap, 0);
						if (newInstance.CheckPosition(tileCountSwap)) {
							Utils.Swap(ref tileCount, ref tileCountSwap);
							roomInstances.Add(newInstance);
							roomsToExtend.Add(newInstance);
						}
					}
				}
			}
			if (!roomInstances.CanGenerate()) return;
			for (int k = 0; k < roomInstances.Count; k++) {
				roomInstances[k].Generate();
			}
		}
		public abstract void Load();
		void ILoadable.Load(Mod mod) {
			Load();
			for (int i = 0; i < rooms.Count; i++) rooms[i].Validate();
		}
		void ILoadable.Unload() { }
	}
	public interface IRoom {
		public string Map { get; }
		public Dictionary<char, TileDescriptor> Key { get; }
		public Dictionary<char, RoomSocket> SocketKey { get; }
		public Range RepetitionRange { get; }
		public bool ValidStart { get; }
		public void PostGenerate(Rectangle area) { }
	}
	public abstract class Room : IRoom {
		public abstract string Map { get; }
		public abstract Dictionary<char, TileDescriptor> Key { get; }
		public abstract Dictionary<char, RoomSocket> SocketKey { get; }
		protected Room() => this.Validate();
		public virtual Range RepetitionRange => 1..1;
		public virtual bool ValidStart => false;
		public virtual void PostGenerate(Rectangle area) { }
	}
	public static class RoomExtensions {
		public static void Validate(this IRoom room) {
			string map = room.Map;
			string[] layers = map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 1; i < layers.Length - 1; i++) {
				if (layers[i].Length != layers[0].Length) throw new Exception($"Invalid layout map {map}, all layers must have the same length");
			}
			HashSet<char> usedConnections = [];
			for (int i = 0; i < map.Length; i++) {
				if (room.SocketKey.ContainsKey(map[i]) && !usedConnections.Add(map[i])) throw new Exception($"Invalid layout map {map}, duplicate connection '{map[i]}'");
			}
		}
		public static Point GetOrigin(this IRoom room, char entrance) {
			string[] map = room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int y = 0; y < map.Length; y++) {
				for (int x = 0; x < map[y].Length; x++) {
					if (map[y][x] == entrance) {
						return new(x, y);
					}
				}
			}
			throw new Exception($"Could not find origin '{entrance}' in map {room.Map}");
		}
		public static bool IsInverse(this Direction direction, Direction other) {
			switch (direction) {
				case Direction.Right:
				return other is Direction.Left or Direction.Horizontal;

				case Direction.Left:
				return other is Direction.Right or Direction.Horizontal;

				case Direction.Horizontal:
				return other is Direction.Left or Direction.Right or Direction.Horizontal;

				case Direction.Up:
				return other is Direction.Down or Direction.Vertical;

				case Direction.Down:
				return other is Direction.Up or Direction.Vertical;

				case Direction.Vertical:
				return other is Direction.Down or Direction.Up or Direction.Vertical;

				default:
				return false;
			}
		}
		public static bool CanGenerate(this List<RoomInstance> rooms) {
			int[] tileCount = new int[Main.maxTilesX * Main.maxTilesY];
			for (int i = 0; i < rooms.Count; i++) {
				if (!rooms[i].CheckPosition(tileCount)) return false;
			}
			return true;
		}
		public static Dictionary<string, List<(IRoom room, char entrance)>[]> GenerateConnectionLookup(this List<IRoom> rooms) {
			Dictionary<string, List<(IRoom room, char entrance)>[]> lookup = [];
			for (int i = 0; i < rooms.Count; i++) {
				IRoom room = rooms[i];
				foreach ((char key, RoomSocket socket) in room.SocketKey) {
					if (!room.Map.Contains(key)) continue;
					if (!lookup.TryGetValue(socket.Key, out List<(IRoom room, char entrance)>[] connections)) {
						lookup[socket.Key] = connections = new List<(IRoom room, char entrance)>[(int)Direction.Vertical + 1];
						for (int j = 0; j < connections.Length; j++) connections[j] = [];
					}
					switch (socket.Direction) {
						case Direction.Right:
						connections[(int)Direction.Left].Add((room, key));
						connections[(int)Direction.Horizontal].Add((room, key));
						break;

						case Direction.Left:
						connections[(int)Direction.Right].Add((room, key));
						connections[(int)Direction.Horizontal].Add((room, key));
						break;

						case Direction.Horizontal:
						connections[(int)Direction.Left].Add((room, key));
						connections[(int)Direction.Right].Add((room, key));
						connections[(int)Direction.Horizontal].Add((room, key));
						break;

						case Direction.Up:
						connections[(int)Direction.Down].Add((room, key));
						connections[(int)Direction.Vertical].Add((room, key));
						break;

						case Direction.Down:
						connections[(int)Direction.Up].Add((room, key));
						connections[(int)Direction.Vertical].Add((room, key));
						break;

						case Direction.Vertical:
						connections[(int)Direction.Down].Add((room, key));
						connections[(int)Direction.Up].Add((room, key));
						connections[(int)Direction.Vertical].Add((room, key));
						break;
					}
				}
			}
			return lookup;
		}
	}
	public enum Direction {
		None,
		Right,
		Left,
		Horizontal,
		Up,
		Down,
		Vertical
	}
	public record class TileDescriptor(Action<int, int> Action, bool Ignore = false) {
		public void DoAction(int i, int j) {
			if (!Ignore) Action(i, j);
		}
		public static TileDescriptor Void => new((_, _) => { }, true);
		public static TileDescriptor Empty => new((i, j) => {
			Tile tile = Main.tile[i, j];
			tile.HasTile = false;
		});
		public static TileDescriptor PlaceTile(ushort type) => new((i, j) => {
			if (type == TileID.MagicalIceBlock) {
				WorldGen.PlaceTile(i, j, TileID.MagicalIceBlock, mute: false, forced: false);
				Projectile.NewProjectile(
					Entity.GetSource_None(),
					new(i * 16 + 8, j * 16 + 8),
					Vector2.Zero,
					ProjectileID.IceBlock,
					0,
					0,
					ai0: -1
				);
				return;
			}
			Tile tile = Main.tile[i, j];
			tile.HasTile = true;
			tile.TileType = type;
		});
	}
	public record class RoomSocket(string Key, Direction Direction, bool Optional = false) {
		public static RoomSocket StartPoint => new("Start", Direction.None, true);
	}
	public record class RoomInstance(IRoom Room, Point Position, HashSet<char> ConsumedConnections, RepetitionData Repetitions = default) {
		public void Generate() {
			string[] map = Room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Position.Deconstruct(out int i, out int j);
			RepetitionData reps = Repetitions;
			int minX = i;
			int minY = j;
			int maxX = i;
			int maxY = j;
			do {
				for (int y = 0; y < map.Length; y++) {
					for (int x = 0; x < map[y].Length; x++) {
						Room.Key[map[y][x]].DoAction(i + x, j + y);
					}
				}
				Min(ref minX, i);
				Min(ref minY, j);
				Max(ref maxX, i + map[0].Length);
				Max(ref maxY, j + map.Length);
			} while (reps.Repeat(ref i, ref j, map[0].Length, map.Length));
			Room.PostGenerate(new(minX, minY, maxX - minX, maxY - minY));
		}
		public bool CheckPosition(int[] tileCounts) {
			string[] map = Room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Position.Deconstruct(out int i, out int j);
			RepetitionData reps = Repetitions;
			Rectangle tileRect = new(0, 0, 1, 1);
			do {
				for (int y = 0; y < map.Length; y++) {
					for (int x = 0; x < map[y].Length; x++) {
						int posIndex = i + x + ((j + y) * Main.maxTilesX);
						Tile existing = Main.tile[i + x, j + y];
						if (existing.HasTile && !TileID.Sets.CanBeClearedDuringGeneration[existing.TileType]) {
							tileCounts[posIndex]++;
						} else if (GenVars.structures is not null) {
							tileRect.X = i + x;
							tileRect.Y = j + y;
							if (!GenVars.structures.CanPlace(tileRect)) tileCounts[posIndex]++;
						}
						if (!Room.Key[map[y][x]].Ignore && ++tileCounts[posIndex] > 1) return false;
					}
				}
			} while (reps.Repeat(ref i, ref j, map[0].Length, map.Length));
			return true;
		}
	}
	public record struct RepetitionData(Direction Direction, int Count) {
		public bool Repeat(ref int i, ref int j, int width, int height) {
			if (--Count < 0) return false;
			switch (Direction) {
				case Direction.Right:
				i += width;
				break;
				case Direction.Left:
				i -= width;
				break;
				case Direction.Up:
				j -= height;
				break;
				case Direction.Down:
				j += height;
				break;

				default:
				return false;
			}
			return true;
		}
	}
	public class TestStructure : Structure {
		public override void Load() {
			rooms.Add(new TestRoom(@"
XXXOTOXXX
XOOOOOOOX
XOOOOOOOX
OOOOOOOOO
OOOOOOOOO
LOOOCOOOR
OOOXXXOOO
OOOOOOOOO
OOOOOOOOO
XOOOOOOOX
XOOOOOOOX
XXXOBOXXX"));
			rooms.Add(new TestRoom(@"
XXXXXXXXX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XXXOBOXXX"));
			rooms.Add(new TestRoom(@"
XXXOTOXXX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XXXXXXXXX"));
			rooms.Add(new TestRoom(@"
XXXXXXXXX
XOOOOOOOX
XOOOOOOOX
XOOOOOOOO
XOOOOOOOO
XOOOOOOOR
XOOOOOOOO
XOOOOOOOO
XOOOOOOOO
XOOOOOOOX
XOOOOOOOX
XXXXXXXXX"));
			rooms.Add(new TestRoom(@"
XXXXXXXXX
XOOOOOOOX
XOOOOOOOX
OOOOOOOOX
OOOOOOOOX
LOOOOOOOX
OOOOOOOOX
OOOOOOOOX
OOOOOOOOX
XOOOOOOOX
XOOOOOOOX
XXXXXXXXX"));
		}
		public class TestRoom : IRoom {
			public string Map { get; }
			public TestRoom(string map) {
				Map = map;
			}
			public Dictionary<char, TileDescriptor> Key { get; } = new() {
				['X'] = TileDescriptor.PlaceTile(TileID.MagicalIceBlock),
				['O'] = TileDescriptor.Empty,
				['C'] = TileDescriptor.Empty,
				['T'] = TileDescriptor.Empty,
				['L'] = TileDescriptor.Empty,
				['R'] = TileDescriptor.Empty,
				['B'] = TileDescriptor.Empty
			};
			public Dictionary<char, RoomSocket> SocketKey { get; } = new() {
				['C'] = RoomSocket.StartPoint,
				['T'] = new("3Wide", Direction.Up, true),
				['B'] = new("3Wide", Direction.Down, true),
				['L'] = new("6Tall", Direction.Left, true),
				['R'] = new("6Tall", Direction.Right, true),
			};
			public Range RepetitionRange { get; } = 1..1;
			public bool ValidStart => Map.Contains('C');

			public void PostGenerate(Rectangle area) {
				for (int x = 0; x < area.Width; x++) {
					byte color = (byte)(x % (PaintID.NegativePaint + 1));
					for (int y = 0; y < area.Height; y++) {
						Tile tile = Main.tile[area.X + x, area.Y + y];
						tile.TileColor = color;
					}
				}
			}
		}
		public class TestRoom2 : IRoom {
			public string Map { get; } =
			@"
XXXXXXXXX
XOOOOOOOX
XOOOOOOOX
OOOOOOOOO
OOOOOOOOO
LOOOCOOOR
OOOOOOOOO
OOOOOOOOO
OOOOOOOOO
XOOOOOOOX
XOOOOOOOX
XXXXXXXXX";
			public Dictionary<char, TileDescriptor> Key { get; } = new() {
				['X'] = TileDescriptor.PlaceTile(TileID.MagicalIceBlock),
				['O'] = TileDescriptor.Empty,
				['C'] = TileDescriptor.Empty,
				['L'] = TileDescriptor.Empty,
				['R'] = TileDescriptor.Empty
			};
			public Dictionary<char, RoomSocket> SocketKey { get; } = new() {
				['C'] = RoomSocket.StartPoint,
				['L'] = new("6Tall", Direction.Left, true),
				['R'] = new("6Tall", Direction.Right, true),
			};
			public Range RepetitionRange { get; } = 1..1;
			public bool ValidStart => true;
			public void PostGenerate(Rectangle area) {
				for (int x = 0; x < area.Width; x++) {
					for (int y = 0; y < area.Height; y++) {
						byte color = (byte)(y % (PaintID.NegativePaint + 1));
						Tile tile = Main.tile[area.X + x, area.Y + y];
						tile.TileColor = color;
					}
				}
			}
		}
	}
}
