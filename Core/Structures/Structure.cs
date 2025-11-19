using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Origins.NPCs;
using PegasusLib.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static Origins.Core.Structures.DeserializedStructure;

namespace Origins.Core.Structures {
	public abstract class Structure {
		public Mod Mod { get; private set; }
		readonly List<ARoom> rooms = [];
		public IReadOnlyList<ARoom> Rooms => rooms;
		public Structure(Mod mod, string fileName) {
			Mod = mod;
			Load();
			KeyValuePair<string, List<(ARoom room, char entrance)>[]>[] lookup = rooms.GenerateConnectionLookup().ToArray();
			List<string> irresolvableSockets = [];
			for (int i = 0; i < lookup.Length; i++) {
				(string name, List<(ARoom room, char entrance)>[] forKey) = lookup[i];
				bool right = forKey[Direction.Right.Index()].Count > 0;
				bool left = forKey[Direction.Left.Index()].Count > 0;
				if (right != left) irresolvableSockets.Add($"{name}_{(left ? Direction.Right : Direction.Left)}");

				bool up = forKey[Direction.Up.Index()].Count > 0;
				bool down = forKey[Direction.Down.Index()].Count > 0;
				if (up != down) irresolvableSockets.Add($"{name}_{(down ? Direction.Up : Direction.Down)}");
			}
			if (irresolvableSockets.Count > 0) throw new FormatException($"{fileName} has irresolvable sockets: [{string.Join(", ", irresolvableSockets)}]");
		}
		public virtual bool IsValidLayout(StructureInstance instance) => true;
		public virtual bool Break(StructureInstance instance) => instance.rooms.Length > 25;
		public void Generate(int i, int j) {
			Dictionary<string, List<(ARoom room, char entrance)>[]> lookup = rooms.GenerateConnectionLookup();
			WeightedRandom<ARoom> start = new(WorldGen.genRand);
			for (int k = 0; k < rooms.Count; k++) {
				if (rooms[k].StartPos != char.MinValue) start.Add(rooms[k]);
			}
			StructureInstance instance;
			while (start.TryPop(out ARoom startRoom)) {
				bool canGenerate = false;
				int tries = 100000;
				do {
					instance = new(this);
					if (!instance.TryAdd(new(startRoom, new Point(i, j) - startRoom.GetOrigin(startRoom.StartPos)), out instance)) break;
					instance = instance.Sprawl(lookup);
				} while (!(canGenerate = instance.CanGenerate()) && --tries > 0);
				if (canGenerate) {
					instance.Generate();
					break;
				}
			}
		}
		public void AddRoom(ARoom room) {
			room.Validate();
			rooms.Add(room);
		}
		public virtual void Load() { }
		public virtual void SetUp() { }
		public static HashSet<Point> ignoreEmpty = [];
	}
	public abstract class ARoom : ModType {
		public string Identifier { get; protected set; }
		public string Map { get; protected set; }
		public Dictionary<char, TileDescriptor> Key { get; protected set; }
		public Dictionary<char, RoomSocket> SocketKey { get; protected set; }
		public Range RepetitionRange { get; protected set; }
		public char StartPos { get; protected set; }
		protected sealed override void Register() {
			ModTypeLookup<ARoom>.Register(this);
		}
		public virtual IEnumerable<Direction> GetRepetitionDirections(Direction connectionDirection) => [connectionDirection];
		public virtual float GetWeight(WeightParameters parameters) => 1;
		public virtual void PostGenerate(PostGenerateParameters parameters) { }
		public virtual string ExportWeight() => null;
		public virtual string ExportPostGenerate() => null;
		public virtual RoomDescriptor Serialize(StructorDescriptor forStructure) => new() { Special = FullName };
		public record struct PostGenerateParameters(RoomInstance Instance, Rectangle Area);
		public record struct WeightParameters(StructureInstance Structure, Point Position);
	}
	public static class RoomExtensions {
		public static void Validate(this ARoom room) {
			string map = room.Map;
			string[] layers = map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 1; i < layers.Length - 1; i++) {
				if (layers[i].Length != layers[0].Length) throw new Exception($"Invalid layout map {map}, all layers must have the same length");
			}
			char[] impossibleSockets = room.SocketKey.Keys.Where(k => !map.Contains(k)).ToArray();
			for (int i = 0; i < impossibleSockets.Length; i++) room.SocketKey.Remove(impossibleSockets[i]);
			HashSet<char> usedConnections = [];
			for (int i = 0; i < map.Length; i++) {
				if (room.SocketKey.ContainsKey(map[i]) && !usedConnections.Add(map[i])) throw new Exception($"Invalid layout map {map}, duplicate connection '{map[i]}'");
			}
		}
		public static Point GetOrigin(this ARoom room, char entrance) {
			string[] map = room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int y = 0; y < map.Length; y++) {
				for (int x = 0; x < map[y].Length; x++) {
					if (map[y][x] == entrance) return new(x, y);
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
		public static bool CanGenerate(this RoomInstance[] rooms) {
			int[] tileCount = new int[Main.maxTilesX * Main.maxTilesY];
			for (int i = 0; i < rooms.Length; i++) if (!rooms[i].CheckPosition(tileCount)) return false;
			return true;
		}
		public static int Index(this Direction direction) => (int)direction - 1;
		public static Dictionary<string, List<(ARoom room, char entrance)>[]> GenerateConnectionLookup(this IReadOnlyList<ARoom> rooms) {
			Dictionary<string, List<(ARoom room, char entrance)>[]> lookup = [];
			for (int i = 0; i < rooms.Count; i++) {
				ARoom room = rooms[i];
				foreach ((char key, RoomSocket socket) in room.SocketKey) {
					if (!room.Map.Contains(key)) continue;
					if (!lookup.TryGetValue(socket.Key, out List<(ARoom room, char entrance)>[] connections)) {
						lookup[socket.Key] = connections = new List<(ARoom room, char entrance)>[(int)Direction.Down];
						for (int j = 0; j < connections.Length; j++) connections[j] = [];
					}
					switch (socket.Direction) {
						case Direction.Right:
						connections[Direction.Left.Index()].Add((room, key));
						break;

						case Direction.Left:
						connections[Direction.Right.Index()].Add((room, key));
						break;

						case Direction.Up:
						connections[Direction.Down.Index()].Add((room, key));
						break;

						case Direction.Down:
						connections[Direction.Up.Index()].Add((room, key));
						break;
					}
				}
			}
			return lookup;
		}
		public static TOut Accumulate<TIn, TOut>(this Accumulator<TIn, TOut> accumulator, TIn input, TOut startValue = default) {
			accumulator?.Invoke(input, ref startValue);
			return startValue;
		}
		public static T IfNotEmpty<T>(this T value, T or = default) where T : ICollection => value.Count > 0 ? value : or;
	}
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Direction {
		Right = 1,
		Left = 2,
		Up = 3,
		Down = 4
	}
	public record class TileDescriptor(Action<RoomInstance, HashSet<char>, int, int> Action, string[] Parts, bool Ignore = false) : ISummable<TileDescriptor> {
		public void DoAction(RoomInstance instance, HashSet<char> connectedSockets, int i, int j) {
			if (!Ignore && Action is not null) Action(instance, connectedSockets, i, j);
		}
		public static TileDescriptor Deserialize(Mod mod, string data) => SerializableTileDescriptor.Create(mod, data);
		static string[] CombineParts(TileDescriptor a, TileDescriptor b) {
			if (b.Ignore || b.Parts is null || b.Parts.Length == 0) return a.Parts;
			if (a.Ignore || a.Parts is null || a.Parts.Length == 0) return b.Parts;
			string[] parts = new string[a.Parts.Length + b.Parts.Length];
			a.Parts.CopyTo(parts, 0);
			b.Parts.CopyTo(parts, a.Parts.Length);
			return parts;
		}
		public override string ToString() => string.Join('+', Parts ?? [nameof(Void)]);
		public static TileDescriptor operator +(TileDescriptor a, TileDescriptor b) => (a is null || b is null) ? (a ?? b) : new(a.Action + b.Action, CombineParts(a, b), a.Ignore && b.Ignore);
	}
	public record class RoomSocket(string Key, Direction Direction, bool Optional = false) {
		public class RoomSocketConverter : JsonConverter {
			public override bool CanConvert(Type objectType) {
				ArgumentNullException.ThrowIfNull(objectType, nameof(objectType));
				return objectType == typeof(RoomSocket);
			}
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				if (reader.TokenType != JsonToken.StartObject) throw new FormatException();
				string[] parts = reader.Value.ToString().Split("..", StringSplitOptions.TrimEntries); // keep empty values
				if (parts.Length != 2) throw new FormatException();
				_ = int.TryParse(parts[0], out int start);
				_ = int.TryParse(parts[1], out int end);
				string Key = null;
				Direction? Direction = null;
				bool Optional = false;
				while (reader.Read()) {
					switch (reader.TokenType) {
						case JsonToken.Comment: break;
						case JsonToken.PropertyName:
						switch (reader.Value.ToString()) {
							case nameof(Key):
							Key = reader.ReadAsString();
							break;

							case nameof(Direction):
							Direction = Enum.Parse<Direction>(reader.ReadAsString());
							break;

							case nameof(Optional):
							Optional = reader.ReadAsBoolean().Value;
							break;
							
							default:
							throw new FormatException($"{reader.Value} is not a valid property name of {nameof(RoomSocket)}");
						}
						break;
						case JsonToken.EndObject:
						if (Key is null) throw new FormatException($"{nameof(Key)} must be specified");
						if (Direction is null) throw new FormatException($"{nameof(Direction)} must be specified");
						return new RoomSocket(Key, Direction.Value, Optional);
						default:
						throw new FormatException();
					}
				}
				throw new FormatException();
			}

			public override void WriteJson(JsonWriter writer, object _value, JsonSerializer serializer) {
				if (_value is not RoomSocket value) throw new NotSupportedException($"{nameof(RoomSocketConverter)} cannot write {_value.GetType()}");
				writer.WriteStartObject();
				writer.WritePropertyName(nameof(Key));
				writer.WriteValue(value.Key);
				writer.WritePropertyName(nameof(Direction));
				writer.WriteValue(value.Direction.ToString());
				writer.WritePropertyName(nameof(Optional));
				writer.WriteValue(value.Optional);
				writer.WriteEndObject();
			}
		}
	}
	public class StructureInstance {
		public readonly RoomInstance[] rooms;
		readonly BitArray overlapTracker;
		readonly SocketTracker socketTracker;
		readonly Structure structure;
		public StructureInstance(Structure structure) : this(structure, []) { }
		StructureInstance(Structure structure, RoomInstance[] rooms, BitArray overlapTracker = null, SocketTracker requiredConnectionTracker = null) {
			this.structure = structure;
			this.rooms = rooms;
			this.overlapTracker = overlapTracker ?? new(Main.maxTilesX * Main.maxTilesY);
			socketTracker = requiredConnectionTracker ?? new();
		}
		public bool CanAdd(RoomInstance room) {
			BitArray newOverlap = new(overlapTracker);
			SocketTracker newSockets = socketTracker.Clone();
			return room.CheckPosition(newOverlap, newSockets);
		}
		public bool TryAdd(RoomInstance room, out StructureInstance result) {
			BitArray newOverlap = new(overlapTracker);
			SocketTracker newSockets = socketTracker.Clone();
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
		public StructureInstance Sprawl(Dictionary<string, List<(ARoom room, char entrance)>[]> lookup) {
			if (structure.Break(this)) return this;
			WeightedRandom<(RoomInstance room, int index)> newRoomOptions = new(WorldGen.genRand);
			newRoomOptions.Clear();
			AddRoomsToExtend(newRoomOptions, lookup);
			if (newRoomOptions.elements.Count <= 0) return this;
			reselect:
			if (newRoomOptions.TryPop(out (RoomInstance room, int index) newRoom)) {
				if (!TryAdd(newRoom.room, out StructureInstance newInstance)) goto reselect;
				StructureInstance finalInstance = newInstance.Sprawl(lookup);
				if (finalInstance == newInstance && !finalInstance.CanGenerate()) {
					goto reselect;
				}
				return finalInstance;
			}
			return this;
		}
		public void AddRoomsToExtend(WeightedRandom<(RoomInstance room, int index)> newRoomOptions, Dictionary<string, List<(ARoom room, char entrance)>[]> lookup) {
			for (int i = 0; i < socketTracker.sockets.Count; i++) {
				(RoomSocket socket, int x, int y) = socketTracker.sockets[i];

				Direction direction = socket.Direction;
				Point pos = new(x, y);
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
				foreach ((ARoom room, char entrance) in lookup[socket.Key][direction.Index()]) {
					float weight = room.GetWeight(new(this, pos));
					if (weight <= 0) continue;
					RoomInstance template = new(room, pos - room.GetOrigin(entrance));
					foreach (Direction repeatDirection in room.GetRepetitionDirections(direction)) {
						for (int j = room.RepetitionRange.Start.Value; j <= room.RepetitionRange.End.Value; j++) {
							RoomInstance newInstance = template with { Repetitions = new(direction, j) };
							if (CanAdd(newInstance)) newRoomOptions.Add((newInstance, i), weight);
						}
					}
				}
			}
		}
		public bool CanGenerate() {
			if (socketTracker.RequiredCount > 0) return false;
			if (!structure.IsValidLayout(this)) return false;
			if (!rooms.CanGenerate()) return false;
			return true;
		}
		public void Generate() {
			for (int k = 0; k < rooms.Length; k++) rooms[k].Generate(socketTracker);
		}
	}
	public record class RoomInstance(ARoom Room, Point Position, RepetitionData Repetitions = default) {
		public void Generate(SocketTracker socketTracker) {
			string[] map = Room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Position.Deconstruct(out int i, out int j);
			RepetitionData reps = Repetitions;
			int minX = i;
			int minY = j;
			int maxX = i;
			int maxY = j;
			List<(char, RoomSocket, Point)> expectedSockets = [];
			foreach (KeyValuePair<char, RoomSocket> soc in Room.SocketKey) expectedSockets.Add((soc.Key, soc.Value, Room.GetOrigin(soc.Key)));
			HashSet<char> connectedSockets = [];
			Structure.ignoreEmpty.Clear();
			do {
				connectedSockets.Clear();
				for (int k = 0; k < expectedSockets.Count; k++) {
					(char key, RoomSocket socket, Point origin) = expectedSockets[k];
					if (!socketTracker.sockets.Contains((socket, i + origin.X, j + origin.Y))) connectedSockets.Add(key);
				}
				for (int y = 0; y < map.Length; y++) {
					for (int x = 0; x < map[y].Length; x++) {
						Room.Key[map[y][x]].DoAction(this, connectedSockets, i + x, j + y);
					}
				}
				Min(ref minX, i);
				Min(ref minY, j);
				Max(ref maxX, i + map[0].Length);
				Max(ref maxY, j + map.Length);
				Structure.ignoreEmpty.Clear();
			} while (reps.Repeat(ref i, ref j, map[0].Length, map.Length));
			Room.PostGenerate(new(this, new(minX, minY, maxX - minX, maxY - minY)));
			if (!WorldGen.generatingWorld) WorldGen.RangeFrame(minX, minY, maxX, maxY);
		}
		public bool CheckPosition(int[] tileCounts) {
			string[] map = Room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Position.Deconstruct(out int i, out int j);
			RepetitionData reps = Repetitions;
			Rectangle tileRect = new(0, 0, 1, 1);
			do {
				for (int y = 0; y < map.Length; y++) {
					for (int x = 0; x < map[y].Length; x++) {
						int posIndex = i + x + (j + y) * Main.maxTilesX;
						Tile existing = Main.tile[i + x, j + y];
						if (existing.HasTile && !TileID.Sets.CanBeClearedDuringGeneration[existing.TileType]) tileCounts[posIndex]++;
						else if (GenVars.structures is not null) {
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
		public bool CheckPosition(BitArray overlapTracker, SocketTracker socketTracker) {
			string[] map = Room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Position.Deconstruct(out int i, out int j);
			RepetitionData reps = Repetitions;
			Rectangle tileRect = new(0, 0, 1, 1);
			do {
				for (int y = 0; y < map.Length; y++) {
					for (int x = 0; x < map[y].Length; x++) {
						int posIndex = i + x + (j + y) * Main.maxTilesX;
						char currentTile = map[y][x];
						if (Room.Key[currentTile].Ignore) continue;
						if (Room.SocketKey.TryGetValue(currentTile, out RoomSocket socket)) socketTracker.Add(socket, i + x, j + y, socket.Optional);
						else if (socketTracker.IsImportant(i + x, j + y)) return false;
						Tile existing = Main.tile[i + x, j + y];
						if (existing.HasTile && !TileID.Sets.CanBeClearedDuringGeneration[existing.TileType]) return false;
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
		readonly List<int> requiredSockets = [];
		public List<(RoomSocket socket, int i, int j)> sockets = [];
		public int RequiredCount => requiredSockets.Count;
		public void Add(RoomSocket socket, int i, int j, bool optional) {
			int x = i;
			int y = j;
			socket.Direction.Nudge(ref x, ref y);
			for (int k = 0; k < sockets.Count; k++) {
				(RoomSocket socket2, int i2, int j2) = sockets[k];
				if (i2 == x && j2 == y) {
					if (socket.Direction.IsInverse(socket2.Direction)) {
						requiredSockets.Remove(k);
						sockets.RemoveAt(k);
					}
					return;
				}
			}
			if (!optional) requiredSockets.Add(sockets.Count);
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
	/*public class TestStructure() : Structure(ModContent.GetInstance<Origins>()) {
		public override void Load() {
			TileDescriptor empty = TileDescriptor.Empty;
			AddRoom(new DeserializedRoom(@"
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
XXXOBOXXX",
new() {
	['X'] = TileDescriptor.PlaceTile(TileID.MagicalIceBlock),
	['O'] = TileDescriptor.Empty,
	['C'] = TileDescriptor.Empty,
	['T'] = TileDescriptor.Empty,
	['L'] = TileDescriptor.Empty,
	['R'] = TileDescriptor.Empty,
	['B'] = TileDescriptor.Empty
}
));
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
			AddRoom(new TestHallway());
			//AddRoom(new TestRequiredRoom());
		}
		public override bool IsValidLayout(StructureInstance instance) => instance.rooms.Any(r => r.Room is TestHallway);
		public class TestRoom(string map) : IRoom {
			public string Map { get; } = map;
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
			public Range RepetitionRange { get; }
			public bool ValidStart => Map.Contains('C');
			public void PostGenerate(RoomInstance instance, Rectangle area) {
				for (int x = 0; x < area.Width; x++) {
					byte color = (byte)(x % (PaintID.NegativePaint + 1));
					for (int y = 0; y < area.Height; y++) {
						Tile tile = Main.tile[area.X + x, area.Y + y];
						tile.TileColor = color;
					}
				}
			}
		}
		public class TestHallway : IRoom {
			public string Map { get; } = @"
XXXX
XOOX
XOOX
OOOO
OOOO
LOOR
OOOO
OOOO
OOOO
XXXX
XOOX
XXXX";
			public Dictionary<char, TileDescriptor> Key { get; } = new() {
				['X'] = TileDescriptor.PlaceTile(TileID.MagicalIceBlock),
				['O'] = TileDescriptor.Empty,
				['L'] = TileDescriptor.Empty,
				['R'] = TileDescriptor.Empty
			};
			public Dictionary<char, RoomSocket> SocketKey { get; } = new() {
				['L'] = new("6Tall", Direction.Left),
				['R'] = new("6Tall", Direction.Right),
			};
			public Range RepetitionRange { get; } = 1..5;
			public bool ValidStart => false;
			public void PostGenerate(RoomInstance instance, Rectangle area) {
				for (int x = 0; x < area.Width; x++) {
					for (int y = 0; y < area.Height; y++) {
						Tile tile = Main.tile[area.X + x, area.Y + y];
						tile.TileColor = PaintID.NegativePaint;
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
				['B'] = new("3Wide", Direction.Down),
				['R'] = new("6Tall", Direction.Right),
			};
			public Range RepetitionRange { get; } = 1..1;
			public bool ValidStart => false;
			public void PostGenerate(RoomInstance instance, Rectangle area) {
				for (int x = 0; x < area.Width; x++) {
					for (int y = 0; y < area.Height; y++) {
						Tile tile = Main.tile[area.X + x, area.Y + y];
						tile.TileColor = PaintID.NegativePaint;
					}
				}
			}
		}
	}*/
}
