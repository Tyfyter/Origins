using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Core.Structures.ARoom;
using static Origins.Core.Structures.DeserializedStructure;

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
		public virtual void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color, bool[,] map, int x, int y, string name) {
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				destination,
				color
			);
		}
		public static new TileDescriptor Create(Mod mod, string data) {
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			TileDescriptor descriptor = null;
			for (int i = 0; i < descriptors.Length; i++) descriptor += CreateSingle(mod, descriptors[i]);
			return descriptor;
		}
		public static string Serialize(Tile tile) => Serializer.Accumulate(tile) ?? "Void";
	}
	public record class PostGenerateDescriptor(Action<PostGenerateParameters> Function, string[] Parts) : ISummable<PostGenerateDescriptor> {
		public void Invoke(PostGenerateParameters parameters) => Function(parameters);
		static string[] CombineParts(PostGenerateDescriptor a, PostGenerateDescriptor b) {
			if (b.Parts is null || b.Parts.Length == 0) return a.Parts;
			if (a.Parts is null || a.Parts.Length == 0) return b.Parts;
			string[] parts = new string[a.Parts.Length + b.Parts.Length];
			a.Parts.CopyTo(parts, 0);
			b.Parts.CopyTo(parts, a.Parts.Length);
			return parts;
		}
		public override string ToString() => string.Join('+', Parts ?? []);
		public static PostGenerateDescriptor operator +(PostGenerateDescriptor a, PostGenerateDescriptor b) => (a is null || b is null) ? (a ?? b) : new(a.Function + b.Function, CombineParts(a, b));
		public static PostGenerateDescriptor Create(Mod mod, string data) => Kind.Create(mod, data);
		public abstract class Kind : SerializableDescriptor<Kind, PostGenerateDescriptor> {
			protected sealed override PostGenerateDescriptor Create(string[] parameters, string originalText) => new(PostGenerate, [originalText]);
			protected sealed override void Register() {
				ModTypeLookup<Kind>.Register(this);
			}
			public abstract void PostGenerate(PostGenerateParameters parameters);
		}
	}
	public record class BreakDescriptor(Accumulator<StructureInstance, bool> Accumulator, string[] Parts) : ISummable<BreakDescriptor>, IAccumulator<StructureInstance, bool> {
		static string[] CombineParts(BreakDescriptor a, BreakDescriptor b) {
			if (b.Parts is null || b.Parts.Length == 0) return a.Parts;
			if (a.Parts is null || a.Parts.Length == 0) return b.Parts;
			string[] parts = new string[a.Parts.Length + b.Parts.Length];
			a.Parts.CopyTo(parts, 0);
			b.Parts.CopyTo(parts, a.Parts.Length);
			return parts;
		}
		public override string ToString() => string.Join('+', Parts ?? []);
		public static BreakDescriptor operator +(BreakDescriptor a, BreakDescriptor b) => (a is null || b is null) ? (a ?? b) : new(a.Accumulator + b.Accumulator, CombineParts(a, b));
		public static BreakDescriptor Create(Mod mod, string data) => Kind.Create(mod, data);
		public abstract class Kind : SerializableDescriptor<Kind, BreakDescriptor> {
			protected sealed override BreakDescriptor Create(string[] parameters, string originalText) => new(Create(parameters), [originalText]);
			protected abstract Accumulator<StructureInstance, bool> Create(string[] parameters);
			protected sealed override void Register() {
				ModTypeLookup<Kind>.Register(this);
			}
		}
	}
	public record class CheckDescriptor(Accumulator<StructureInstance, bool> Accumulator, string[] Parts) : ISummable<CheckDescriptor>, IAccumulator<StructureInstance, bool> {
		static string[] CombineParts(CheckDescriptor a, CheckDescriptor b) {
			if (b.Parts is null || b.Parts.Length == 0) return a.Parts;
			if (a.Parts is null || a.Parts.Length == 0) return b.Parts;
			string[] parts = new string[a.Parts.Length + b.Parts.Length];
			a.Parts.CopyTo(parts, 0);
			b.Parts.CopyTo(parts, a.Parts.Length);
			return parts;
		}
		public override string ToString() => string.Join('+', Parts ?? []);
		public static CheckDescriptor operator +(CheckDescriptor a, CheckDescriptor b) => (a is null || b is null) ? (a ?? b) : new(a.Accumulator + b.Accumulator, CombineParts(a, b));
		public static CheckDescriptor Create(Mod mod, string data) => Kind.Create(mod, data);
		public abstract class Kind : SerializableDescriptor<Kind, CheckDescriptor> {
			protected sealed override CheckDescriptor Create(string[] parameters, string originalText) => new(Create(parameters), [originalText]);
			protected abstract Accumulator<StructureInstance, bool> Create(string[] parameters);
			protected sealed override void Register() {
				ModTypeLookup<Kind>.Register(this);
			}
		}
	}
	public record class WeightDescriptor(Accumulator<WeightParameters, float> Accumulator, string[] Parts) : ISummable<WeightDescriptor>, IAccumulator<WeightParameters, float> {
		static string[] CombineParts(WeightDescriptor a, WeightDescriptor b) {
			if (b.Parts is null || b.Parts.Length == 0) return a.Parts;
			if (a.Parts is null || a.Parts.Length == 0) return b.Parts;
			string[] parts = new string[a.Parts.Length + b.Parts.Length];
			a.Parts.CopyTo(parts, 0);
			b.Parts.CopyTo(parts, a.Parts.Length);
			return parts;
		}
		public override string ToString() => string.Join('*', Parts ?? []);
		public static WeightDescriptor operator +(WeightDescriptor a, WeightDescriptor b) => (a is null || b is null) ? (a ?? b) : new(a.Accumulator + b.Accumulator, CombineParts(a, b));
		public static WeightDescriptor Create(Mod mod, string data) => Kind.Create(mod, data);
		public abstract class Kind : SerializableDescriptor<Kind, WeightDescriptor> {
			protected sealed override void Register() {
				ModTypeLookup<Kind>.Register(this);
			}
			protected sealed override WeightDescriptor Create(string[] parameters, string originalText) => new(Create(parameters), [originalText]);
			protected abstract Accumulator<WeightParameters, float> Create(string[] parameters);
			public static new WeightDescriptor Create(Mod mod, string data) {
				if (string.IsNullOrWhiteSpace(data)) return null;
				string[] descriptors = data.Split('*', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				WeightDescriptor descriptor = null;
				for (int i = 0; i < descriptors.Length; i++) {
					if (float.TryParse(descriptors[i], out float constant)) {
						descriptor += new WeightDescriptor((WeightParameters _, ref float output) => output *= constant, [descriptors[i]]);
					} else {
						descriptor += CreateSingle(mod, descriptors[i]);
					}
				}
				return descriptor;
			}
		}
	}
	public abstract class SerializableDescriptor<TSelf, T> : ModType where TSelf : SerializableDescriptor<TSelf, T> where T : IAdditionOperators<T, T, T> {
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
		public static T Create(Mod mod, string data) {
			if (string.IsNullOrWhiteSpace(data)) return default;
			string[] descriptors = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			T descriptor = default;
			for (int i = 0; i < descriptors.Length; i++) descriptor += CreateSingle(mod, descriptors[i]);
			return descriptor;
		}
		protected static T CreateSingle(Mod mod, string data) {
			if (!TryGet(mod, data, out TSelf source, out string[] parameters)) throw new ArgumentException($"{data} is not a properly formatted tile descriptor, if you can't read the regex, see https://regex101.com/r/17DyBY/1", nameof(data));
			return source.Create(parameters, data);
		}
	}
	[Autoload(false)]
	public class DeserializedRoom : ARoom {
		readonly PostGenerateDescriptor postGenerate;
		readonly WeightDescriptor weight;
		readonly HashSet<string> tags;
		public DeserializedRoom(Mod mod, RoomDescriptor descriptor) {
			Identifier = descriptor.Identifier;
			Map = string.Join('\n', descriptor.Map);
			Key = descriptor.Key.Select<KeyValuePair<char, string>, KeyValuePair<char, TileDescriptor>>(v => new(v.Key, TileDescriptor.Deserialize(mod, v.Value))).ToDictionary();
			SocketKey = descriptor.SocketKey ?? [];
			RepetitionRange = descriptor.RepetitionRange;
			StartPos = descriptor.StartPos;
			postGenerate = PostGenerateDescriptor.Create(mod, descriptor.PostGenerate);
			weight = WeightDescriptor.Create(mod, descriptor.Weight);
			tags = (descriptor.Tags ?? []).ToHashSet();
		}
		public override RoomDescriptor Serialize(StructorDescriptor forStructure) => new() {
			Map = Map.Split('\n', StringSplitOptions.RemoveEmptyEntries),
			Key = new Dictionary<char, string>(Key.TrySelect((KeyValuePair<char, TileDescriptor> element, out KeyValuePair<char, string> pattern) => {
				pattern = new(element.Key, element.Value.ToString());
				if (forStructure?.TileDescriptors is null) return true;
				return !forStructure.TileDescriptors.TryGetValue(element.Key, out string global) || global != pattern.Value;
			})).IfNotEmpty(),
			SocketKey = SocketKey,
			RepetitionRange = RepetitionRange,
			StartPos = StartPos,
			PostGenerate = ExportPostGenerate(),
			Weight = ExportWeight(),
			Tags = (tags ?? []).ToArray(),
		};
		public override void PostGenerate(PostGenerateParameters parameters) => postGenerate?.Invoke(parameters);
		public override float GetWeight(WeightParameters parameters) => weight.Accumulate(parameters, 1);
		public override string ExportPostGenerate() => postGenerate?.ToString();
		public override string ExportWeight() => weight?.ToString();
	}
	[Autoload(false)]
	public class DeserializedStructure(Mod mod, string fileName, string data) : Structure(mod, fileName) {
		public static JsonSerializerSettings SerializerSettings { get; } = new() {
			Formatting = Formatting.Indented,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			NullValueHandling = NullValueHandling.Ignore
		};
		BreakDescriptor breakCondition;
		CheckDescriptor isValidLayout;
		public override void Load() {
			StructorDescriptor descriptor = new();
			JsonConvert.PopulateObject(data, descriptor, DeserializedStructure.SerializerSettings);
			if (descriptor.BreakCondition is not null) breakCondition = BreakDescriptor.Create(Mod, descriptor.BreakCondition);
			if (descriptor.ValidLayoutCheck is not null) isValidLayout = CheckDescriptor.Create(Mod, descriptor.ValidLayoutCheck);
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
			breakCondition ??= BreakDescriptor.Create(Mod, $"{nameof(MaxRoomCount)}({1000})");
		}
		public string Export() {
			StructorDescriptor descriptor = new() {
				Rooms = [],
				BreakCondition = breakCondition?.ToString(),
				ValidLayoutCheck = isValidLayout?.ToString(),
			};
			{
				CompressorMap<char, string> sharedKeys = new();
				for (int i = 0; i < Rooms.Count; i++) {
					foreach (KeyValuePair<char, TileDescriptor> item in Rooms[i].Key) {
						sharedKeys.Add(item.Key, item.Value.ToString());
					}
				}
				descriptor.TileDescriptors = sharedKeys.Export();
			}
			{
				for (int i = 0; i < Rooms.Count; i++) {
					ARoom room = Rooms[i];
					descriptor.Rooms.Add(room.Identifier, room.Serialize(descriptor));
				}
			}
			return RoomExtensions.SerializeObject(descriptor);
		}
		public static Task<DeserializedStructure> AsyncLoad(string fileName) => Task.Run(() => Load(fileName));
		public static DeserializedStructure Load(string fileName) {
			if (!fileName.EndsWith(".json")) fileName += ".json";
			string sourcePath = Path.Combine([Program.SavePathShared, "ModSources", fileName.Replace('/', Path.DirectorySeparatorChar)]);
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
			public string Special;
			public string[] Map;
			public Dictionary<char, string> Key;
			public Dictionary<char, RoomSocket> SocketKey;
			[JsonConverter(typeof(RangeConverter))]
			public Range RepetitionRange = default;
			[DefaultValue(char.MinValue)]
			public char StartPos = char.MinValue;
			public string PostGenerate;
			public string Weight;
			public string[] Tags;
			public ARoom CreateRoom(Mod mod) {
				if (!string.IsNullOrWhiteSpace(Special)) {
					string[] parts = Special.Split('/');
					string modName = parts[0];
					if (parts.Length < 2) modName = mod.Name;
					string name = parts[^1];
					if (!ModContent.TryFind($"{modName}/{name}", out ARoom room)) room = ModContent.Find<ARoom>($"Origins/{name}");
					return room;
				}
				return new DeserializedRoom(mod, this);
			}
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
					writer.WriteValue($"{value.Start.Value}..{value.End.Value}");
				}
			}
		}
	}
	public delegate void Accumulator<TIn, TOut>(TIn input, ref TOut output);
	public interface IAccumulator<TIn, TOut> {
		Accumulator<TIn, TOut> Accumulator { get; }
	}
}
