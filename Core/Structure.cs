using PegasusLib.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Origins.Core {
	public abstract class Structure : ILoadable {
		readonly List<IRoom> rooms = [];
		public virtual bool IsValidLayout(StructureInstance instance) => true;
		public void Generate(int i, int j) {
			Dictionary<string, List<(IRoom room, char entrance)>[]> lookup = rooms.GenerateConnectionLookup();
			WeightedRandom<IRoom> start = new(WorldGen.genRand);
			for (int k = 0; k < rooms.Count; k++) {
				if (rooms[k].ValidStart) start.Add(rooms[k]);
			}
			IRoom startRoom = start.Get();
			StructureInstance instance;
			do {
				instance = new(this);
				instance.TryAdd(new(startRoom, new Point(i, j) - startRoom.GetOrigin('C'), []), out instance);
				instance = instance.Sprawl(lookup);
			} while (!instance.CanGenerate());
			instance.Generate();
		}
		public void AddRoom(IRoom room) {
			room.Validate();
			rooms.Add(room);
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
			char[] impossibleSockets = room.SocketKey.Keys.Where(k => !map.Contains(k)).ToArray();
			for (int i = 0; i < impossibleSockets.Length; i++) {
				room.SocketKey.Remove(impossibleSockets[i]);
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
				return other == Direction.Left;

				case Direction.Left:
				return other == Direction.Right;

				case Direction.Up:
				return other == Direction.Down;

				case Direction.Down:
				return other == Direction.Up;

				default:
				return false;
			}
		}
		public static void Nudge(this Direction direction, ref int x, ref int y) {
			switch (direction) {
				case Direction.Right:
				x++;
				break;
				case Direction.Left:
				x--;
				break;
				case Direction.Up:
				y--;
				break;
				case Direction.Down:
				y++;
				break;
			}
		}
		public static bool CanGenerate(this List<RoomInstance> rooms) {
			int[] tileCount = new int[Main.maxTilesX * Main.maxTilesY];
			for (int i = 0; i < rooms.Count; i++) {
				if (!rooms[i].CheckPosition(tileCount)) return false;
			}
			return true;
		}
		public static bool CanGenerate(this RoomInstance[] rooms) {
			int[] tileCount = new int[Main.maxTilesX * Main.maxTilesY];
			for (int i = 0; i < rooms.Length; i++) {
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
						lookup[socket.Key] = connections = new List<(IRoom room, char entrance)>[(int)Direction.Down + 1];
						for (int j = 0; j < connections.Length; j++) connections[j] = [];
					}
					switch (socket.Direction) {
						case Direction.Right:
						connections[(int)Direction.Left].Add((room, key));
						break;

						case Direction.Left:
						connections[(int)Direction.Right].Add((room, key));
						break;

						case Direction.Up:
						connections[(int)Direction.Down].Add((room, key));
						break;

						case Direction.Down:
						connections[(int)Direction.Up].Add((room, key));
						break;
					}
				}
			}
			return lookup;
		}
		public static void ConsumeConnection(this RoomInstance[] rooms, int index, char exit, RoomInstance consumedBy) {
			Dictionary<char, RoomInstance> consumedConnections = rooms[index].ConsumedConnections.ToDictionary();
			consumedConnections.Add(exit, consumedBy);
			rooms[index] = rooms[index] with { ConsumedConnections = consumedConnections };
		}
	}
	public enum Direction {
		Right,
		Left,
		Up,
		Down
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
	public record class RoomSocket(string Key, Direction Direction, bool Optional = false);
	public class StructureInstance {
		public readonly RoomInstance[] rooms;
		readonly BitArray overlapTracker;
		readonly SocketTracker requiredConnectionTracker;
		readonly Structure structure;
		public StructureInstance(Structure structure) : this(structure, []) { }
		StructureInstance(Structure structure, RoomInstance[] rooms, BitArray overlapTracker = null, SocketTracker requiredConnectionTracker = null) {
			this.structure = structure;
			this.rooms = rooms;
			this.overlapTracker = overlapTracker ?? new(Main.maxTilesX * Main.maxTilesY);
			this.requiredConnectionTracker = requiredConnectionTracker ?? new();
		}
		public bool CanAdd(RoomInstance room) {
			BitArray newOverlap = new(overlapTracker);
			SocketTracker newSockets = requiredConnectionTracker.Clone();
			return room.CheckPosition(newOverlap, newSockets);
		}
		public bool TryAdd(RoomInstance room, out StructureInstance result) {
			BitArray newOverlap = new(overlapTracker);
			SocketTracker newSockets = requiredConnectionTracker.Clone();
			if (!room.CheckPosition(newOverlap, newSockets)) {
				result = this;
				return false;
			}
			RoomInstance[] newRooms = new RoomInstance[rooms.Length + 1];
			rooms.CopyTo(newRooms, 0);
			newRooms[^1] = room;
			StructureInstance structureInstance = new(structure, newRooms, newOverlap, newSockets);
			result = structureInstance;
			return true;
		}
		public StructureInstance Sprawl(Dictionary<string, List<(IRoom room, char entrance)>[]> lookup) {
			if (rooms.Length > 25) return this;
			WeightedRandom<(RoomInstance room, int index, char exit)> newRoomOptions = new(WorldGen.genRand);
			newRoomOptions.Clear();
			AddRoomsToExtend(newRoomOptions, lookup);
			if (newRoomOptions.elements.Count <= 0) return this;
			reselect:
			if (newRoomOptions.TryPop(out (RoomInstance room, int index, char exit) newRoom)) {
				if (!TryAdd(newRoom.room, out StructureInstance newInstance)) goto reselect;
				newInstance.rooms.ConsumeConnection(newRoom.index, newRoom.exit, newRoom.room);
				return newInstance.Sprawl(lookup);
			}
			return this;
		}
		public void AddRoomsToExtend(WeightedRandom<(RoomInstance room, int index, char exit)> newRoomOptions, Dictionary<string, List<(IRoom room, char entrance)>[]> lookup) {
			for (int i = 0; i < rooms.Length; i++) {
				RoomInstance currentRoom = rooms[i];
				double weight = 1.0 / currentRoom.Room.SocketKey.Values.Count;
				foreach (KeyValuePair<char, RoomSocket> socket in currentRoom.Room.SocketKey) {
					if (currentRoom.ConsumedConnections.ContainsKey(socket.Key)) continue;

					Direction direction = socket.Value.Direction;
					Point pos = currentRoom.Position + currentRoom.Room.GetOrigin(socket.Key);
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
					foreach ((IRoom room, char entrance) in lookup[socket.Value.Key][(int)direction]) {
						RoomInstance newInstance = new(room, pos - room.GetOrigin(entrance), new() { [entrance] = currentRoom });
						if (CanAdd(newInstance)) newRoomOptions.Add((newInstance, i, socket.Key), weight);
					}
				}
			}
		}
		public bool CanGenerate() {
			if (requiredConnectionTracker.Count > 0) return false;
			if (!structure.IsValidLayout(this)) return false;
			if (!rooms.CanGenerate()) return false;
			return true;
		}
		public void Generate() {
			for (int k = 0; k < rooms.Length; k++) {
				rooms[k].Generate();
			}
		}
	}
	public record class RoomInstance(IRoom Room, Point Position, Dictionary<char, RoomInstance> ConsumedConnections, RepetitionData Repetitions = default) {
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
		public bool CheckPosition(BitArray overlapTracker, SocketTracker requiredConnectionTracker) {
			string[] map = Room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Position.Deconstruct(out int i, out int j);
			RepetitionData reps = Repetitions;
			Rectangle tileRect = new(0, 0, 1, 1);
			do {
				for (int y = 0; y < map.Length; y++) {
					for (int x = 0; x < map[y].Length; x++) {
						int posIndex = i + x + ((j + y) * Main.maxTilesX);
						char currentTile = map[y][x];
						if (Room.Key[currentTile].Ignore) continue;
						if (Room.SocketKey.TryGetValue(currentTile, out RoomSocket socket)) {
							requiredConnectionTracker.Add(socket, i + x, j + y);
						} else if (requiredConnectionTracker.IsImportant(i + x, j + y)) return false;
						Tile existing = Main.tile[i + x, j + y];
						if (existing.HasTile && !TileID.Sets.CanBeClearedDuringGeneration[existing.TileType]) {
							return false;
						}
						if (GenVars.structures is not null) {
							tileRect.X = i + x;
							tileRect.Y = j + y;
							if (!GenVars.structures.CanPlace(tileRect)) return false;
						}
						if (overlapTracker[posIndex]) return false;
						overlapTracker[posIndex] = true;
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
	public class SocketTracker {
		List<(RoomSocket socket, int i, int j)> sockets = [];
		public int Count => sockets.Count;
		public void Add(RoomSocket socket, int i, int j) {
			int x = i;
			int y = j;
			socket.Direction.Nudge(ref x, ref y);
			for (int k = 0; k < sockets.Count; k++) {
				(RoomSocket socket2, int i2, int j2) = sockets[k];
				if (i2 == x && j2 == y) {
					if (socket.Direction.IsInverse(socket2.Direction)) sockets.RemoveAt(k);
					return;
				}
			}
			sockets.Add((socket, i, j));
		}
		public bool IsImportant(int i, int j) {
			for (int k = 0; k < sockets.Count; k++) {
				(RoomSocket socket, int i2, int j2) = sockets[k];
				socket.Direction.Nudge(ref i2, ref j2);
				if (i2 == i && j2 == j) return true;
			}
			return false;
		}
		public SocketTracker Clone() => new() { sockets = sockets.ToList() };
	}
	public class TestStructure : Structure {
		public override void Load() {
			AddRoom(new TestRoom(@"
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
			AddRoom(new TestRoom(@"
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
			AddRoom(new TestRoom(@"
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
			AddRoom(new TestRoom(@"
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
			AddRoom(new TestRoom(@"
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
			AddRoom(new TestRequiredRoom());
		}
		public override bool IsValidLayout(StructureInstance instance) => instance.rooms.Any(r => r.Room is TestRequiredRoom);
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
				['T'] = new("3Wide", Direction.Up),
				['B'] = new("3Wide", Direction.Down),
				['L'] = new("6Tall", Direction.Left),
				['R'] = new("6Tall", Direction.Right),
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
		public class TestRequiredRoom : IRoom {
			public string Map { get; } = @"
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
XXXOBOXXX";
			public Dictionary<char, TileDescriptor> Key { get; } = new() {
				['X'] = TileDescriptor.PlaceTile(TileID.MagicalIceBlock),
				['O'] = TileDescriptor.Empty,
				['R'] = TileDescriptor.Empty,
				['B'] = TileDescriptor.Empty
			};
			public Dictionary<char, RoomSocket> SocketKey { get; } = new() {
				['C'] = RoomSocket.StartPoint,
				['B'] = new("3Wide", Direction.Down),
				['R'] = new("6Tall", Direction.Right),
			};
			public Range RepetitionRange { get; } = 1..1;
			public bool ValidStart => false;
			public void PostGenerate(Rectangle area) {
				for (int x = 0; x < area.Width; x++) {
					for (int y = 0; y < area.Height; y++) {
						Tile tile = Main.tile[area.X + x, area.Y + y];
						tile.TileColor = PaintID.NegativePaint;
					}
				}
			}
		}
	}
}
