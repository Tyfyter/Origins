using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			Accumulator condition = null;
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
				bool canGenerate = true;
				condition(connectedSockets, ref canGenerate);
				if (canGenerate) {
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = type;
				}
			});
		}
		delegate void Accumulator(HashSet<char> connectedSockets, ref bool canGenerate);
	}
	public class PlaceTile : SerializableTileDescriptor {
		readonly Dictionary<string, TileDescriptor> descriptors = [];
		protected override TileDescriptor Create(string[] parameters) {
			if (!descriptors.TryGetValue(parameters[0], out TileDescriptor descriptor)) {
				CachedTileType type = new(parameters[0]);
				descriptor = new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = type;
				});
				descriptors[parameters[0]] = descriptor;
			}
			return descriptor;
		}
		public class CachedTileType(string name) {
			readonly string name = name;
			ushort? id;
			public ushort Value => id ??= (ushort)TileID.Search.GetId(name);
			public static implicit operator ushort(CachedTileType type) => type.Value;
		}
	}
	public class PlaceWall : SerializableTileDescriptor {
		readonly Dictionary<string, TileDescriptor> descriptors = [];
		protected override TileDescriptor Create(string[] parameters) {
			if (!descriptors.TryGetValue(parameters[0], out TileDescriptor descriptor)) {
				CachedWallType type = new(parameters[0]);
				descriptor = new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.WallType = type;
				});
				descriptors[parameters[0]] = descriptor;
			}
			return descriptor;
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
	public abstract class SerializableTileDescriptor : ModType {
		protected abstract TileDescriptor Create(string[] parameters);
		protected override void Register() {
			ModTypeLookup<SerializableTileDescriptor>.Register(this);
		}
		static Regex parse = new("(?:(\\w+)/)?(\\w+)(?:\\((.*)\\))?", RegexOptions.Compiled);
		public static TileDescriptor Create(Mod mod, string data) {
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			TileDescriptor descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) descriptor += CreateSingle(mod, descriptors[i]);
			return descriptor;
		}
		static TileDescriptor CreateSingle(Mod mod, string data) {
			Match match = parse.Match(data);
			if (!match.Success) throw new ArgumentException($"{data} is not a properly formatted tile descriptor, if you can't read the regex, see https://regex101.com/r/17DyBY/1", nameof(data));
			string modName = match.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(modName)) modName = mod.Name;
			string name = match.Groups[2].Value;
			string[] parameters = match.Groups[3].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (!ModContent.TryFind($"{modName}/{name}", out SerializableTileDescriptor source)) source = ModContent.Find<SerializableTileDescriptor>($"Origins/{name}");
			return source.Create(parameters);
		}
	}
	public class DeserializedRoom(string identifier, string map, Dictionary<char, TileDescriptor> key, Dictionary<char, RoomSocket> socketKey, Range repetitionRange = default, bool validStart = false) : IRoom {
		public string Identifier { get; } = identifier;
		public string Map { get; } = map;
		public Dictionary<char, TileDescriptor> Key { get; } = key;
		public Dictionary<char, RoomSocket> SocketKey { get; } = socketKey;
		public Range RepetitionRange { get; } = repetitionRange;
		public bool ValidStart { get; } = validStart;
		public void PostGenerate(RoomInstance instance, Rectangle area) {
			/*for (int x = 0; x < area.Width; x++) {
				byte color = (byte)(x % (PaintID.NegativePaint + 1));
				for (int y = 0; y < area.Height; y++) {
					Tile tile = Main.tile[area.X + x, area.Y + y];
					tile.TileColor = color;
				}
			}*/
		}
	}
	[Autoload(false)]
	public class DeserializedStructure(Mod mod, string data) : Structure(mod) {
		public override void Load() {
			StructorDescriptor descriptor = new();
			JsonConvert.PopulateObject(data, descriptor, new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				NullValueHandling = NullValueHandling.Ignore
			});
			descriptor.TileDescriptors ??= [];
			foreach (string key in descriptor.TileDescriptors.Keys) {
				if (key.Length != 1) throw new FormatException($"Invalid tile descriptor key '{key}', keys must be a single character");
			}
			RoomDescriptor[] rooms = new RoomDescriptor[descriptor.Rooms.Count];
			int i = 0;
			foreach ((string identifier, RoomDescriptor room) in descriptor.Rooms) {
				room.Identifier = identifier;
				if (room.Key is null) {
					room.Key = descriptor.TileDescriptors;
				} else {
					foreach (string key in room.Key.Keys) throw new FormatException($"Invalid tile descriptor key '{key}', keys must be a single character");
					foreach ((string signifier, string tile) in descriptor.TileDescriptors) room.Key.TryAdd(signifier, tile);
				}
				rooms[i++] = room;
			}
			for (i = 0; i < rooms.Length; i++) {
				AddRoom(rooms[i].CreateRoom(Mod));
			}
		}
		public static Task<DeserializedStructure> AsyncLoad(string fileName) => Task.Run(() => Load(fileName));
		public static DeserializedStructure Load(string fileName) {
			return new DeserializedStructure(ModLoader.GetMod(fileName.Split('/')[0]), Encoding.UTF8.GetString(ModContent.GetFileBytes(fileName + ".json")));
		}
		public class StructorDescriptor {
			public Dictionary<string, RoomDescriptor> Rooms;
			public Dictionary<string, string> TileDescriptors;
		}
		public class RoomDescriptor {
			public string Identifier;
			public string[] Map;
			public Dictionary<string, string> Key;
			public Dictionary<char, RoomSocket> SocketKey;
			public Range RepetitionRange = default;
			public bool ValidStart = false;
			public DeserializedRoom CreateRoom(Mod mod) {
				return new(
					Identifier,
					string.Join('\n', Map),
					Key.Select<KeyValuePair<string, string>, KeyValuePair<char, TileDescriptor>>(v => new(v.Key[0], TileDescriptor.Deserialize(mod, v.Value))).ToDictionary(),
					SocketKey ?? [],
					RepetitionRange,
					ValidStart
				);
			}
		}
	}
}
