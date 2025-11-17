using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Core.Structures.DeserializedStructure;
using static Origins.Core.Structures.IRoom;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Origins.Core.Structures {
	public abstract class SerializableTileDescriptor : SerializableDescriptor<SerializableTileDescriptor, TileDescriptor> {
		static Accumulator<Tile, string> Serializer = null;
		protected static void AddSerializer(Func<Tile, string> serializer) {
			Serializer += (Tile tile, ref string data) => {
				string newDesc = serializer(tile);
				if (string.IsNullOrWhiteSpace(newDesc)) return;
				if (data is null) {
					data = newDesc;
				} else {
					data += "+" + newDesc;
				}
			};
		}
		protected sealed override void Register() {
			ModTypeLookup<SerializableTileDescriptor>.Register(this);
		}
		public static TileDescriptor Create(Mod mod, string data) {
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			TileDescriptor descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) descriptor += CreateSingle(mod, descriptors[i]);
			return descriptor;
		}
		public static string Serialize(Tile tile) => Serializer.Accumulate(tile) ?? "Void";
	}
	public abstract class PostGenerateDescriptor : ModType {
		protected sealed override void Register() {
			ModTypeLookup<PostGenerateDescriptor>.Register(this);
		}
		public static Action<PostGenerateParameters> Parse(Mod mod, string data) {
			if (string.IsNullOrWhiteSpace(data)) return null;
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			Action<PostGenerateParameters> descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) {
				string[] parts = descriptors[i].Split('/');
				string modName = parts[0];
				if (parts.Length < 2) modName = mod.Name;
				string name = parts[^1];
				if (!ModContent.TryFind($"{modName}/{name}", out PostGenerateDescriptor source)) source = ModContent.Find<PostGenerateDescriptor>($"Origins/{name}");
				descriptor += source.PostGenerate;
			}
			return descriptor;
		}
		public abstract void PostGenerate(PostGenerateParameters parameters);
	}
	public abstract class SerializableBreakDescriptor : SerializableDescriptor<SerializableBreakDescriptor, Accumulator<StructureInstance, bool>> {
		protected sealed override void Register() {
			ModTypeLookup<SerializableBreakDescriptor>.Register(this);
		}
		public static Accumulator<StructureInstance, bool> Create(Mod mod, string data) {
			if (string.IsNullOrWhiteSpace(data)) return null;
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			Accumulator<StructureInstance, bool> descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) descriptor += CreateSingle(mod, descriptors[i]);
			return descriptor;
		}
	}
	public abstract class SerializableCheckDescriptor : SerializableDescriptor<SerializableCheckDescriptor, Accumulator<StructureInstance, bool>> {
		protected sealed override void Register() {
			ModTypeLookup<SerializableCheckDescriptor>.Register(this);
		}
		public static Accumulator<StructureInstance, bool> Create(Mod mod, string data) {
			if (string.IsNullOrWhiteSpace(data)) return null;
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			Accumulator<StructureInstance, bool> descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) descriptor += CreateSingle(mod, descriptors[i]);
			return descriptor;
		}
	}
	public abstract class WeightDescriptor : SerializableDescriptor<WeightDescriptor, Accumulator<WeightParameters, float>> {
		protected sealed override void Register() {
			ModTypeLookup<WeightDescriptor>.Register(this);
		}
		public static Accumulator<WeightParameters, float> Create(Mod mod, string data) {
			if (string.IsNullOrWhiteSpace(data)) return null;
			string[] descriptors = data.Split('*', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			Accumulator<WeightParameters, float> descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) {
				if (float.TryParse(descriptors[i], out float constant)) {
					descriptor += (WeightParameters _, ref float output) => output *= constant;
				} else {
					descriptor += CreateSingle(mod, descriptors[i]);
				}
			}
			return descriptor;
		}
	}
	public abstract class SerializableDescriptor<TSelf, T> : ModType where TSelf : SerializableDescriptor<TSelf, T> {
		protected abstract T Create(string[] parameters, string originalText);
		static readonly Regex parse = new("(?:(\\w+)/)?(\\w+)(?:\\((.*)\\))?", RegexOptions.Compiled);
		public static bool TryParse(Mod mod, string data, out string modName, out string name, out string[] parameters) {
			Match match = parse.Match(data);
			if (!match.Success) {
				modName = default;
				name = default;
				parameters = default;
				return false;
			}
			modName = match.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(modName)) modName = mod.Name;
			name = match.Groups[2].Value;
			parameters = match.Groups[3].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			return true;
		}
		public static TSelf Get(string modName, string name) {
			if (!ModContent.TryFind($"{modName}/{name}", out TSelf source)) source = ModContent.Find<TSelf>($"Origins/{name}");
			return source;
		}
		public static bool TryGet(Mod mod, string data, out TSelf source, out string[] parameters) {
			if (!TryParse(mod, data, out string modName, out string name, out parameters)) {
				source = default;
				return false;
			}
			source = Get(modName, name);
			return true;
		}
		public virtual IEnumerable<(string name, Color color)> GetDisplayLayers(string[] parameters) => [];
		protected static T CreateSingle(Mod mod, string data) {
			if (!TryGet(mod, data, out TSelf source, out string[] parameters)) throw new ArgumentException($"{data} is not a properly formatted tile descriptor, if you can't read the regex, see https://regex101.com/r/17DyBY/1", nameof(data));
			return source.Create(parameters, data);
		}
	}
	public class DeserializedRoom(Mod mod, RoomDescriptor descriptor) : IRoom {
		public string Identifier { get; } = descriptor.Identifier;
		public string Map { get; } = string.Join('\n', descriptor.Map);
		public Dictionary<char, TileDescriptor> Key { get; } = descriptor.Key.Select<KeyValuePair<char, string>, KeyValuePair<char, TileDescriptor>>(v => new(v.Key, TileDescriptor.Deserialize(mod, v.Value))).ToDictionary();
		public Dictionary<char, RoomSocket> SocketKey { get; } = descriptor.SocketKey ?? [];
		public Range RepetitionRange { get; } = descriptor.RepetitionRange;
		public char StartPos { get; } = descriptor.StartPos;
		readonly Action<PostGenerateParameters> postGenerate = PostGenerateDescriptor.Parse(mod, descriptor.PostGenerate);
		public void PostGenerate(PostGenerateParameters parameters) => postGenerate?.Invoke(parameters);
		readonly Accumulator<WeightParameters, float> weight = WeightDescriptor.Create(mod, descriptor.Weight);
		public float GetWeight(WeightParameters parameters) => weight.Accumulate(parameters, 1);
	}
	[Autoload(false)]
	public class DeserializedStructure(Mod mod, string fileName, string data) : Structure(mod, fileName) {
		Accumulator<StructureInstance, bool> breakCondition;
		Accumulator<StructureInstance, bool> isValidLayout;
		public override void Load() {
			StructorDescriptor descriptor = new();
			JsonConvert.PopulateObject(data, descriptor, new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				NullValueHandling = NullValueHandling.Ignore
			});
			if (descriptor.BreakCondition is not null) breakCondition = SerializableBreakDescriptor.Create(Mod, descriptor.BreakCondition);
			if (descriptor.ValidLayoutCheck is not null) isValidLayout = SerializableCheckDescriptor.Create(Mod, descriptor.ValidLayoutCheck);
			descriptor.TileDescriptors ??= [];
			RoomDescriptor[] rooms = new RoomDescriptor[descriptor.Rooms.Count];
			int i = 0;
			foreach ((string identifier, RoomDescriptor room) in descriptor.Rooms) {
				room.Identifier = identifier;
				if (room.Key is null) {
					room.Key = descriptor.TileDescriptors;
				} else {
					foreach ((char signifier, string tile) in descriptor.TileDescriptors) room.Key.TryAdd(signifier, tile);
				}
				rooms[i++] = room;
			}
			for (i = 0; i < rooms.Length; i++) {
				AddRoom(new DeserializedRoom(Mod, rooms[i]));
			}
			breakCondition ??= MaxRoomCount.Create(1000);
		}
		public static Task<DeserializedStructure> AsyncLoad(string fileName) => Task.Run(() => Load(fileName));
		public static DeserializedStructure Load(string fileName) {
			if (!fileName.EndsWith(".json")) fileName += ".json";
			string sourcePath = Path.Combine([Program.SavePathShared, "ModSources", fileName]);
			return new DeserializedStructure(
				ModLoader.GetMod(fileName.Split('/')[0]),
				Path.GetFileNameWithoutExtension(fileName),
				File.Exists(sourcePath) ? File.ReadAllText(sourcePath) : Encoding.UTF8.GetString(ModContent.GetFileBytes(fileName))
			);
		}
		public override bool Break(StructureInstance instance) => breakCondition.Accumulate(instance);
		public override bool IsValidLayout(StructureInstance instance) => isValidLayout.Accumulate(instance, true);
		public class StructorDescriptor {
			public Dictionary<string, RoomDescriptor> Rooms;
			public Dictionary<char, string> TileDescriptors;
			public string BreakCondition;
			public string ValidLayoutCheck;
		}
		public class RoomDescriptor {
			public string Identifier;
			public string[] Map;
			public Dictionary<char, string> Key;
			public Dictionary<char, RoomSocket> SocketKey;
			[JsonConverter(typeof(RangeConverter))]
			public Range RepetitionRange = default;
			public char StartPos = char.MinValue;
			public string PostGenerate;
			public string Weight;
			public class RangeConverter : JsonConverter {
				public override bool CanConvert(Type objectType) {
					ArgumentNullException.ThrowIfNull(objectType, nameof(objectType));
					return objectType == typeof(Range);
				}
				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
					if (reader.TokenType != JsonToken.String) throw new FormatException();
					string[] parts = reader.Value.ToString().Split("..", StringSplitOptions.TrimEntries); // keep empty values
					if (parts.Length != 2) throw new FormatException();
					_ = int.TryParse(parts[0], out int start);
					_ = int.TryParse(parts[1], out int end);
					return new Range(start, end);
				}

				public override void WriteJson(JsonWriter writer, object _value, JsonSerializer serializer) {
					if (_value is not Range value) throw new NotSupportedException($"{nameof(RangeConverter)} cannot write {_value.GetType()}");
					writer.WriteValue($"{value.Start.Value}.{value.End.Value}");
				}
			}
		}
	}
	public delegate void Accumulator<TIn, TOut>(TIn input, ref TOut output);
}
