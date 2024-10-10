using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origins.Items.Weapons.Ammo;
using Origins.NPCs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Dev.WikiPageExporter;

namespace Origins.Dev {
	public class WikiPageExporter : ILoadable {
		public delegate string WikiLinkFormatter(string name);
		public static DictionaryWithNull<Mod, WikiLinkFormatter> LinkFormatters { get; private set; } = [];
		static List<(Type, WikiProvider)> typedDataProviders;
		public static List<(Type type, WikiProvider provider)> TypedDataProviders => typedDataProviders ??= [];
		public delegate bool ConditionalWikiProvider(object obj, out WikiProvider provider, ref bool replaceGenericClassProvider);
		static List<ConditionalWikiProvider> conditionalDataProviders;
		public static List<ConditionalWikiProvider> ConditionalDataProviders => conditionalDataProviders ??= [];
		static HashSet<Type> interfaceReplacesGenericClassProvider;
		public static HashSet<Type> InterfaceReplacesGenericClassProvider => interfaceReplacesGenericClassProvider ??= [];
		public void Load(Mod mod) {
			LinkFormatters[Origins.instance] = (t) => {
				string formattedName = t.Replace(" ", "_");
				return $"<a is=a-link image=$fromStats>{t}</a>";
			};
			LinkFormatters[null] = (t) => {
				string formattedName = t.Replace(" ", "_");
				return $"<a is=a-link href=https://terraria.wiki.gg/wiki/{formattedName}>{t}</a>";
			};
		}
		static PageTemplate wikiTemplate;
		static DateTime wikiTemplateWriteTime;
		public static PageTemplate WikiTemplate {
			get {
				if (wikiTemplate is null || File.GetLastWriteTime(DebugConfig.Instance.WikiTemplatePath) > wikiTemplateWriteTime) {
					wikiTemplate = new(File.ReadAllText(DebugConfig.Instance.WikiTemplatePath));
					wikiTemplateWriteTime = File.GetLastWriteTime(DebugConfig.Instance.WikiTemplatePath);
				}
				return wikiTemplate;
			}
		}
		static void TryUpdatingExistingPage(string pagePath, PageTemplate template, Dictionary<string, object> context) {
			List<PageTemplate.ConditionalSnippit> conditionals = template.GetConditionals();
			string oldPageText = File.ReadAllText(pagePath);
			HTMLDocument pageNodes = HTMLDocument.Parse(oldPageText);
			for (int i = 0; i < conditionals.Count; i++) {
				HTMLDocument newNodes = HTMLDocument.Parse(conditionals[i].Resolve(context));
				if (newNodes.Children.Count == 1 && newNodes.Children[0] is HTMLNode singleNode && singleNode.attributes.TryGetValue("id", out string id) && pageNodes.GetElementByID(id) is HTMLNode replaceable) {
					replaceable.Parent.ReplaceChild(replaceable, singleNode);
				}
			}
			string newPageText = pageNodes.ToString().ReplaceLineEndings("\n");
			if (newPageText != oldPageText) {
				WriteFileNoUnneededRewrites(pagePath, newPageText);
			}
		}
		public static void ExportItemPage(Item item) {
			foreach (WikiProvider provider in GetWikiProviders(item.ModItem)) {
				string pagePath = GetWikiPagePath(provider.PageName(item.ModItem));
				(PageTemplate template, Dictionary<string, object> context) = provider.GetPage(item.ModItem);
				if (context == null) continue;
				if (((item.ModItem as ICustomWikiStat)?.FullyGeneratable ?? false) || !File.Exists(pagePath)) {
					WriteFileNoUnneededRewrites(GetWikiPagePath(provider.PageName(item.ModItem)), template.Resolve(context));
				} else {
					TryUpdatingExistingPage(pagePath, template, context);
				}
			}
		}
		public static void ExportItemStats(Item item) {
			foreach (WikiProvider provider in GetWikiProviders(item.ModItem)) {
				foreach ((string name, JObject stats) stats in provider.GetStats(item.ModItem)) {
					WriteFileNoUnneededRewrites(
						GetWikiStatPath(stats.name),
						JsonConvert.SerializeObject(stats.stats, Formatting.Indented)
					);
				}
			}
		}
		public static void ExportNPCPage(NPC npc) {
			foreach (WikiProvider provider in GetWikiProviders(npc.ModNPC)) {
				if (provider.GetPage(npc.ModNPC) is (PageTemplate template, Dictionary<string, object> context) && context is not null) {
					WriteFileNoUnneededRewrites(GetWikiPagePath(provider.PageName(npc.ModNPC)), template.Resolve(context));
				}
			}
		}
		public static void ExportNPCStats(NPC npc) {
			foreach (WikiProvider provider in GetWikiProviders(npc.ModNPC)) {
				foreach ((string name, JObject stats) stats in provider.GetStats(npc.ModNPC)) {
					WriteFileNoUnneededRewrites(
						GetWikiStatPath(stats.name),
						JsonConvert.SerializeObject(stats.stats, Formatting.Indented)
					);
				}
			}
		}
		static UInt32[] CRCTable;
		static bool crc_table_computed = false;
		static string currentCRCText = "";
		static int currentCRCLength = 0;
		public static void ExportContentSprites(object content) {
			int screenWidth = Main.screenWidth;
			int screenHeight = Main.screenHeight;
			foreach (WikiProvider provider in GetWikiProviders(content)) {
				foreach ((string name, Texture2D texture) in provider.GetSprites(content) ?? Array.Empty<(string, Texture2D)>()) {
					string filePath = Path.Combine(DebugConfig.Instance.WikiSpritesPath, name) + ".png";
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));
					MemoryStream stream = new(texture.Width * texture.Height * 4);
					texture.SaveAsPng(stream, texture.Width, texture.Height);
					texture.Dispose();
					if (File.Exists(filePath)) {
						FileStream fileStream = File.OpenRead(filePath);
						bool isChanged = fileStream.Length != stream.Length;
						for (int i = 0; i < stream.Length && !isChanged; i++) {
							if (stream.ReadByte() != fileStream.ReadByte()) isChanged = true;
						}
						fileStream.Close();
						if (isChanged) {
							fileStream = File.OpenWrite(filePath);
							stream.Position = 0;
							fileStream.Position = 0;
							stream.WriteTo(fileStream);
							fileStream.Close();
						}
					} else {
						FileStream fileStream = File.Create(filePath);
						stream.WriteTo(fileStream);
						fileStream.Close();
					}
					stream.Close();
				}
				static void make_crc_table() {
					CRCTable = new uint[256];
					uint c;
					short n, k;

					for (n = 0; n < 256; n++) {
						c = (uint)n;
						for (k = 0; k < 8; k++) {
							if ((c & 1) != 0)
								c = 0xedb88320u ^ (c >> 1);
							else
								c >>= 1;
						}
						CRCTable[n] = c;
					}
					crc_table_computed = true;
				}
				static void FinalizeCRC(uint crc32, Stream stream) {
					crc32 ^= 0xFFFFFFFFu;
					stream.Write(ToBytes((uint)crc32));
#if DEBUG
					Origins.instance.Logger.Info(currentCRCLength - 4 + ":" + currentCRCText);
					currentCRCText = "";
					currentCRCLength = 0;
#endif
				}
				static uint ReadUInt(byte[] data, int position) {
					return ((uint)data[position + 0] << 8 * 3) | ((uint)data[position + 1] << 8 * 2) | ((uint)data[position + 2] << 8 * 1) | ((uint)data[position + 3] << 8 * 0);
				}
				static byte[] ToBytes(uint data) {
					return [(byte)(data >> 8 * 3 & 0xFF), (byte)(data >> 8 * 2 & 0xFF), (byte)(data >> 8 * 1 & 0xFF), (byte)(data >> 8 * 0 & 0xFF)];
				}
				static byte[] UShortToBytes(ushort data) {
					return [(byte)(data >> 8 * 1 & 0xFF), (byte)(data >> 8 * 0 & 0xFF)];
				}
				static int NextChunk(int cursor, byte[] buffer, params byte[] targetName) {
					uint targetBytes = 0;
					string chunkName = null;
					if (targetName.Length > 0) {
						chunkName = string.Join("", targetName.Select(c => (char)c));
						if (targetName.Length != 4) {
							throw new ArgumentException($"Target chunk type must be 4 bytes, {chunkName} is {targetName.Length} bytes", nameof(targetName));
						}
						targetBytes = ReadUInt(targetName, 0);
						if (targetBytes == ReadUInt(buffer, cursor + 4)) return cursor;
					}
					start:
					int chunkLength = (int)ReadUInt(buffer, cursor);
					/* length + chunk type + data + crc*/
					cursor += 4 + 4 + chunkLength + 4;
					if (targetName.Length > 0) {
						if (targetBytes != ReadUInt(buffer, cursor + 4)) goto start;
					}
					return cursor;
				}
				static void WriteAndAdvanceCRC(Stream stream, ref uint crc, byte[] buffer, int offset = 0, int length = -1, string name = "") {
#if DEBUG
					currentCRCText += $"\n{name} :";
#endif
					if (!crc_table_computed) make_crc_table();
					if (length == -1) length = buffer.Length - offset;
					stream.Write(buffer, offset, length);
					for (int i = 0; i < length; i++) {
						crc = CRCTable[(crc ^ buffer[i + offset]) & 0xff] ^ (crc >> 8);
						if (currentCRCText.Length < 4) {
							currentCRCText += (char)buffer[i + offset];
						} else {
							string h = buffer[i + offset].ToString("X");
							currentCRCText += " " + (h.Length == 1 ? "0" + h : h);
						}
#if DEBUG
						currentCRCLength++;
#endif
					}
#if DEBUG
					currentCRCText += "|";
#endif
				}
				foreach ((string name, (Texture2D texture, int frames)[] textures) in provider.GetAnimatedSprites(content) ?? Array.Empty<(string, (Texture2D texture, int frames)[])>()) {
					string filePath = Path.Combine(DebugConfig.Instance.WikiSpritesPath, name) + ".png";
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));
					FileStream stream = File.Create(filePath);
					int seqNum = 0;
					for (int i = 0; i < textures.Length; i++) {
						Texture2D texture = textures[i].texture;
						int size = texture.Width * texture.Height * 4;
						MemoryStream memoryStream = new(0);//never actually large enough, but 
						texture.SaveAsPng(memoryStream, texture.Width, texture.Height);
						//texture.SaveAsPng(File.Create(Path.Combine(DebugConfig.Instance.WikiSpritesPath, sprite.name) + $"_frame_{i}.png"), texture.Width, texture.Height);
						texture.Dispose();
						byte[] buffer = memoryStream.GetBuffer();
						const int _endSig = 16;
						const int _width = _endSig;
						const int _height = _width + 4;
						const int _bit_depth = _height + 4;
						const int _color_type = _bit_depth + 1;
						//const int _compression = _color_type + 1;
						//const int _filter = _compression + 1;
						//const int _interlace = _filter + 1;
						string test = "";
						string hex = "";
						for (int j = 0; j < buffer.Length; j++) {
							test += (char)buffer[j];
							string h = buffer[j].ToString("X");
							hex += (h.Length == 1 ? "0" + h : h) + "";
							if (j % 16 == 15) {
								//test += '\n';
								//hex += '\n';
							}
						}
						if (buffer[_color_type] != 6) Origins.LogError("invalid color type " + buffer[_color_type]);
						int IDAT_pos = NextChunk(0x08, buffer, "IDAT"u8.ToArray());
						uint IDAT_len = ReadUInt(buffer, IDAT_pos);
						uint crc32 = 0xFFFFFFFFu;
						if (i == 0) {
							stream.Write(buffer, 0, IDAT_pos);
							stream.Write([0x00, 0x00, 0x00, 0x08], 0, 4);
							WriteAndAdvanceCRC(stream, ref crc32, [
								/*acTL      */0x61, 0x63, 0x54, 0x4C,
								/*num_frames*/0x00, 0x00, 0x00, (byte)textures.Length,
								/*num_plays */0x00, 0x00, 0x00, 0xFF
							]);
							FinalizeCRC((uint)crc32, stream);
						}

						stream.Write([
							0x00, 0x00, 0x00, 0x1A
						], 0, 4);
						crc32 = 0xFFFFFFFFu;
						WriteAndAdvanceCRC(stream, ref crc32, "fcTL"u8.ToArray());
						//BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true);
						//writer.Flush();
						//writer.Dispose();

						WriteAndAdvanceCRC(stream, ref crc32, ToBytes((uint)seqNum++), name: "sequence_number");
						WriteAndAdvanceCRC(stream, ref crc32, ToBytes((uint)texture.Width), name: "width");
						WriteAndAdvanceCRC(stream, ref crc32, ToBytes((uint)texture.Height), name: "height");
						WriteAndAdvanceCRC(stream, ref crc32, BitConverter.GetBytes((uint)0), name: "x_offset");
						WriteAndAdvanceCRC(stream, ref crc32, BitConverter.GetBytes((uint)0), name: "y_offset");
						WriteAndAdvanceCRC(stream, ref crc32, UShortToBytes((ushort)textures[i].frames), name: "delay_num");
						WriteAndAdvanceCRC(stream, ref crc32, UShortToBytes((ushort)60), name: "delay_den");
						WriteAndAdvanceCRC(stream, ref crc32, [(byte)1], name: "dispose_op");//APNG_DISPOSE_OP_BACKGROUND
						WriteAndAdvanceCRC(stream, ref crc32, [(byte)0], name: "blend_op");//APNG_BLEND_OP_SOURCE
						FinalizeCRC((uint)crc32, stream);

						crc32 = 0xFFFFFFFFu;
						if (i != 0) {
							stream.Write(ToBytes((uint)(IDAT_len + 4)));
							WriteAndAdvanceCRC(stream, ref crc32, [
								/*fdAT          */0x66, 0x64, 0x41, 0x54,
								/*sequence_number*/0x00, 0x00, 0x00, (byte)seqNum++
							]);
						} else {
							stream.Write(ToBytes((uint)IDAT_len));
							WriteAndAdvanceCRC(stream, ref crc32, "IDAT"u8.ToArray());
						}
						WriteAndAdvanceCRC(stream, ref crc32, buffer, IDAT_pos + 4 + 4, (int)IDAT_len);
						FinalizeCRC((uint)crc32, stream);

						if (i == textures.Length - 1) {
							int IEND_pos = NextChunk(IDAT_pos, buffer, "IEND"u8.ToArray());
							uint IEND_len = ReadUInt(buffer, IEND_pos);
							stream.Write(buffer, IEND_pos, 4 + 4 + (int)IEND_len + 4);
						}
					}
					stream.Close();
				}
			}
			Main.screenWidth = screenWidth;
			Main.screenHeight = screenHeight;
		}
		public static string GetWikiName(ModItem modItem) => modItem.Name.Replace("_Item", "");//maybe switch to WebUtility.UrlEncode(modItem.DisplayName.Value);
		public static string GetWikiName(ModNPC modNPC) => modNPC.Name;//maybe switch to WebUtility.UrlEncode(modItem.DisplayName.Value);
		public static string GetWikiPagePath(string name) => Path.Combine(DebugConfig.Instance.WikiPagePath, name + ".html");
		public static string GetWikiStatPath(string name) => Path.Combine(DebugConfig.Instance.StatJSONPath, name + ".json");
		public static string GetWikiItemImagePath(ModItem modItem) => Main.itemAnimations[modItem.Type] is not null ? modItem.Name.Replace(' ', '_') : modItem.Texture.Replace(modItem.Mod.Name, "§ModImage§");
		public static string GetWikiItemRarity(Item item) => (RarityLoader.GetRarity(item.rare)?.Name ?? ItemRarityID.Search.GetName(item.rare)).Replace("Rarity", "");
		public void Unload() {
			wikiTemplate = null;
			LinkFormatters = null;
			typedDataProviders = null;
			conditionalDataProviders = null;
			interfaceReplacesGenericClassProvider = null;
		}
		public static void WriteFileNoUnneededRewrites(string file, string text) {
			if (File.Exists(file) && File.ReadAllText(file) == text) return;
			File.WriteAllText(file, text);
		}
		public static LocalizedText GetDefaultMainPageText(ILocalizedModType modType) {
			string name = modType.Name;
			if (modType is ModItem modItem) name = GetWikiName(modItem);
			return Language.GetOrRegister($"WikiGenerator.{modType.Mod.Name}.{modType.LocalizationCategory}.{name}.MainText");
		}
		public static IEnumerable<(string name, LocalizedText text)> GetDefaultPageTexts(ILocalizedModType modType) {
			string baseKey = $"WikiGenerator.{modType?.Mod?.Name}.{modType?.LocalizationCategory}.{modType?.Name}.";
			return GetDefaultPageTexts(baseKey);
		}
		public static IEnumerable<(string name, LocalizedText text)> GetDefaultPageTexts(string baseKey) {
			LocalizedText text;
			bool TryGetText(string suffix, out LocalizedText text) {
				if (Language.Exists(baseKey + suffix)) {
					text = Language.GetText(baseKey + suffix);
					return true;
				}
				text = null;
				return false;
			}
			if (TryGetText("Behavior", out text)) yield return ("Behavior", text);
			if (TryGetText("DifficultyChanges", out text)) yield return ("DifficultyChanges", text);
			if (TryGetText("Tips", out text)) yield return ("Tips", text);
			if (TryGetText("Trivia", out text)) yield return ("Trivia", text);
			if (TryGetText("Notes", out text)) yield return ("Notes", text);
			if (TryGetText("Lore", out text)) yield return ("Lore", text);
			if (TryGetText("Changelog", out text)) yield return ("Changelog", text);
		}
		public static IEnumerable<WikiProvider> GetDefaultProviders(object obj) {
			Type type = obj.GetType();
			(Type type, WikiProvider provider) bestMatch = default;
			bool noClassProvider = false;
			foreach ((Type type, WikiProvider provider) item in TypedDataProviders) {
				if (item.type.IsAssignableFrom(type)) {
					if (item.type.IsInterface) {
						if (InterfaceReplacesGenericClassProvider.Contains(item.type)) noClassProvider = true;
						yield return item.provider;
					} else if (bestMatch.type is null || item.type.IsAssignableTo(bestMatch.type)) {
						bestMatch = item;
					}
				}
			}
			foreach (ConditionalWikiProvider item in ConditionalDataProviders) {
				if (item(obj, out WikiProvider provider, ref noClassProvider)) {
					yield return provider;
				}
			}
			if (noClassProvider && bestMatch.type != type) yield break;
			yield return bestMatch.provider;
		}
		public static IEnumerable<WikiProvider> GetWikiProviders(object obj) {
			if (obj is ICustomWikiStat stat) {
				return stat.GetWikiProviders();
			}
			return GetDefaultProviders(obj);
		}
		public static void AddTorchLightStats(JObject data, Vector3 light) {
			float intensity = MathF.Max(MathF.Max(light.X, light.Y), light.Z);
			if (intensity > 1) light /= intensity;
			data.Add("LightIntensity", $"{intensity * 100:0.#}%");
			data.Add("LightColor", new JArray() { light.X, light.Y, light.Z });
		}
	}
	public class PageTemplate(string text) {
		readonly ITemplateSnippet[] snippets = Parse(text);

		public string Resolve(Dictionary<string, object> context) => CombineSnippets(snippets, context).ReplaceLineEndings("\n");
		public static ITemplateSnippet[] Parse(string text) {
			StringBuilder currentText = new();
			int blockDepth = 0;
			List<ITemplateSnippet> snippets = [];
			void FlushText() {
				if (currentText.Length > 0) {
					snippets.Add(new PlainTextSnippit(currentText.ToString()));
					currentText.Clear();
				}
			}
			for (int i = 0; i < text.Length; i++) {
				bool append = true;
				switch (text[i]) {
					case '§':
					append = false;
					FlushText();
					i++;
					while (text[i] != '§') currentText.Append(text[i++]);
					snippets.Add(new VariableSnippit(currentText.ToString()));
					currentText.Clear();
					break;

					case '#': {
						append = false;
						FlushText();
						i++;
						bool isFor = false;
						while (!char.IsWhiteSpace(text[i])) currentText.Append(text[i++]);
						switch (currentText.ToString()) {
							case "if":
							blockDepth += 1;
							break;
							case "for":
							blockDepth += 1;
							isFor = true;
							break;
							case "endif":
							throw new ArgumentException($"invalid format, endif with no if at character {i}: {text}");
							case "endfor":
							throw new ArgumentException($"invalid format, endfor with no for at character {i}: {text}");
						}
						currentText.Clear();

						i++;
						while (text[i] != '\n') currentText.Append(text[i++]);
						string condition = currentText.ToString();
						currentText.Clear();

						while (blockDepth > 0) {
							switch (text[i]) {
								case '#': {
									int parsePos = i + 1;
									StringBuilder directiveParser = new();
									while (!char.IsWhiteSpace(text[parsePos])) directiveParser.Append(text[parsePos++]);
									switch (directiveParser.ToString()) {
										case "if":
										blockDepth += 1;
										break;
										case "for":
										blockDepth += 1;
										break;
										case "endif":
										blockDepth -= 1;
										break;
										case "endfor":
										blockDepth -= 1;
										break;
									}
									if (blockDepth != 0) goto default;
									i += (isFor ? "endfor" : "endif").Length;
									break;
								}

								default:
								currentText.Append(text[i++]);
								break;
							}
						}
						snippets.Add(
							isFor ?
							new RepeatingSnippet(condition, Parse(currentText.ToString())) :
							new ConditionalSnippit(condition, Parse(currentText.ToString()))
						);
						currentText.Clear();
					}
					break;
				}
				if (append) {
					currentText.Append(text[i]);
				}
			}
			FlushText();
			return snippets.ToArray();
		}
		public List<ConditionalSnippit> GetConditionals() {
			List<ConditionalSnippit> conditionals = [];
			for (int i = 0; i < snippets.Length; i++) {
				if (snippets[i] is ConditionalSnippit conditional) conditionals.Add(conditional);
			}
			return conditionals;
		}
		static string CombineSnippets(ITemplateSnippet[] snippets, Dictionary<string, object> context) => string.Join(null, snippets.Select(s => s.Resolve(context)));
		public interface ITemplateSnippet {
			string Resolve(Dictionary<string, object> context);
		}
		public class PlainTextSnippit(string text) : ITemplateSnippet {
			public readonly string text = text;
			public string Resolve(Dictionary<string, object> context) => text;
		}
		public class ConditionalSnippit(string condition, ITemplateSnippet[] snippets) : ITemplateSnippet {
			public readonly string condition = condition;
			public readonly ITemplateSnippet[] snippets = snippets;

			public string Resolve(Dictionary<string, object> context) {
				if (context.ContainsKey(condition)) return CombineSnippets(snippets, context);
				return null;
			}
		}
		public class RepeatingSnippet(string name, ITemplateSnippet[] snippets) : ITemplateSnippet {
			public readonly string name = name;
			public readonly ITemplateSnippet[] snippets = snippets;

			public string Resolve(Dictionary<string, object> context) {
				StringBuilder builder = new();
				if (context.TryGetValue(name, out object value) && value is IEnumerable enumerable) {
					foreach (var item in enumerable) {
						context["iteratorValue"] = item;
						builder.Append(CombineSnippets(snippets, context));
					}
				}
				return builder.ToString();
			}
		}
		public class VariableSnippit(string name) : ITemplateSnippet {
			public readonly string name = name;

			public string Resolve(Dictionary<string, object> context) {
				object value = context[name];
				if (value is List<Recipe> recipes) {
					StringBuilder builder = new();
					builder.AppendLine("<a-recipes>");
					foreach (var group in recipes.GroupBy((r) => new RecipeRequirements(r))) {
						builder.AppendLine("{");
						if (group.Key.requirements.Length > 0) {
							builder.AppendLine("stations:[");
							foreach ((Mod reqMod, string reqName) in group.Key.requirements) {
								if (LinkFormatters.TryGetValue(reqMod, out WikiLinkFormatter formatter)) {
									builder.AppendLine($"`{formatter(reqName)}`");
								} else {
									Origins.instance.Logger.Warn($"No wiki link formatter for mod {reqMod}, skipping requirement for {reqName}");
								}
							}
							builder.AppendLine("],");
						}
						builder.AppendLine("items:[");
						bool first = true;
						foreach (Recipe recipe in group) {
							if (!first) builder.Append(',');
							first = false;
							builder.AppendLine("{");
							builder.AppendLine("result:`" + WikiExtensions.GetItemText(recipe.createItem) + "`,");
							builder.AppendLine("ingredients:[");
							for (int i = 0; i < recipe.requiredItem.Count; i++) {
								if (i > 0) builder.Append(',');
								builder.AppendLine($"`{WikiExtensions.GetItemText(recipe.requiredItem[i])}`");
							}
							builder.AppendLine("]");
							builder.AppendLine("}");
						}
						builder.AppendLine("]");
						builder.AppendLine("}");
					}
					builder.Append("</a-recipes>");
					return builder.ToString();
				}
				return CombineSnippets(Parse(value.ToString()), context);
			}
		}
	}
	public class RecipeRequirements(Recipe recipe) {
		public readonly (Mod mod, string name)[] requirements =
				recipe.requiredTile.Select(t => (TileLoader.GetTile(t)?.Mod, Lang.GetMapObjectName(MapHelper.TileToLookup(t, 0))))
				/*.Concat(
					recipe.Conditions.Select(c => c.Description.Value)
				)*/.ToArray();

		public override bool Equals(object other) {
			return other is RecipeRequirements req && Equals(req);
		}
		public bool Equals(RecipeRequirements other) {
			if (other.requirements.Length != requirements.Length) return false;
			for (int i = 0; i < requirements.Length; i++) {
				if (!other.requirements.Contains(requirements[i])) return false;
			}
			return true;
		}

		public override int GetHashCode() {
			unchecked {
				int hashCode = 0;
				for (int i = 0; i < requirements.Length; i++) {
					hashCode = (hashCode * 397) ^ requirements[i].GetHashCode();
				}
				return hashCode;
			}
		}
	}
	public class DictionaryWithNull<TKey, TValue> : Dictionary<TKey, TValue> {
		bool hasNullValue;
		TValue nullValue;
		public new TValue this[TKey key] {
			get {
				if (key is null) return nullValue;
				return base[key];
			}
			set {
				if (key is null) {
					nullValue = value;
					hasNullValue = true;
				} else {
					base[key] = value;
				}
			}
		}
		public new bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
			if (key is null) {
				value = nullValue;
				return hasNullValue;
			}
			return base.TryGetValue(key, out value);
		}
	}
	public interface ICustomWikiStat {
		bool Buyable => false;
		void ModifyWikiStats(JObject data) { }
		string[] Categories => Array.Empty<string>();
		bool? Hardmode => null;
		bool FullyGeneratable => false;
		bool ShouldHavePage => true;
		bool NeedsCustomSprite => false;
		string CustomSpritePath => null;
		LocalizedText PageTextMain => (this is ILocalizedModType modType) ? WikiPageExporter.GetDefaultMainPageText(modType) : null;
		IEnumerable<(string name, LocalizedText text)> PageTexts => (this is ILocalizedModType modType) ? WikiPageExporter.GetDefaultPageTexts(modType) : null;
		IEnumerable<WikiProvider> GetWikiProviders() => WikiPageExporter.GetDefaultProviders(this);
		IEnumerable<string> LocalizedStats => [];
	}
	public class ItemWikiProvider : WikiProvider<ModItem> {
		public override string PageName(ModItem modItem) => WikiPageExporter.GetWikiName(modItem);
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(ModItem modItem) {
			Item item = modItem.Item;
			Dictionary<string, object> context = new() {
				["Name"] = WikiPageExporter.GetWikiName(modItem),
				["DisplayName"] = Lang.GetItemNameValue(item.type)
			};
			(List<Recipe> recipes, List<Recipe> usedIn) = WikiExtensions.GetRecipes(item);
			if (recipes.Count > 0 || usedIn.Count > 0) {
				context["Crafting"] = true;
				if (recipes.Count > 0) {
					context["Recipes"] = recipes;
				}
				if (usedIn.Count > 0) {
					context["UsedIn"] = usedIn;
				}
			}

			IEnumerable<(string name, LocalizedText text)> pageTexts;
			if (modItem is ICustomWikiStat wikiStats) {
				context["PageTextMain"] = wikiStats.PageTextMain.Value;
				pageTexts = wikiStats.PageTexts;
			} else {
				string key = $"WikiGenerator.{modItem.Mod?.Name}.{modItem.LocalizationCategory}.{context["Name"]}.MainText";
				context["PageTextMain"] = Language.GetOrRegister(key).Value;
				pageTexts = WikiPageExporter.GetDefaultPageTexts(modItem);
			}
			foreach (var text in pageTexts) {
				context[text.name] = text.text;
			}
			return (WikiTemplate, context);
		}
		public override IEnumerable<(string, JObject)> GetStats(ModItem modItem) {
			Item item = modItem.Item;
			JObject data = [];
			ICustomWikiStat customStat = item.ModItem as ICustomWikiStat;
			data["Image"] = customStat?.CustomSpritePath ?? WikiPageExporter.GetWikiItemImagePath(modItem);
			data["Name"] = item.Name;
			JArray types = new("Item");
			if (item.accessory) types.Add("Accessory");
			if (item.damage > 0 && item.useStyle != ItemUseStyleID.None) {
				types.Add("Weapon");
				if (!item.noMelee && item.useStyle == ItemUseStyleID.Swing) types.Add("Sword");
				if (item.shoot > ProjectileID.None) {
					switch (ContentSamples.ProjectilesByType[item.shoot].aiStyle) {
						case ProjAIStyleID.Boomerang:
						types.Add("Boomerang");
						break;

						case ProjAIStyleID.Spear:
						types.Add("Spear");
						break;
					}
				}
				switch (item.useAmmo) {
					case ItemID.WoodenArrow:
					types.Add("Bow");
					break;
					case ItemID.MusketBall:
					types.Add("Gun");
					break;

					default:
					if (item.useAmmo == ModContent.ItemType<Metal_Slug>()) types.Add("Handcannon");
					else if (item.useAmmo == ModContent.ItemType<Harpoon>()) types.Add("HarpoonGun");
					break;
				}
				switch (item.ammo) {
					case ItemID.WoodenArrow:
					types.Add("Arrow");
					break;
					case ItemID.MusketBall:
					types.Add("Bullet");
					break;

					default:
					if (item.ammo == ModContent.ItemType<Harpoon>()) types.Add("Harpoon");
					break;
				}
			}
			if (customStat?.Hardmode ?? (!item.consumable && item.rare > ItemRarityID.Orange)) types.Add("Hardmode");

			if (ItemID.Sets.IsFood[item.type]) types.Add("Food");
			if (item.ammo != 0) types.Add("Ammo");
			if (item.pick != 0 || item.axe != 0 || item.hammer != 0 || item.fishingPole != 0 || item.bait != 0) types.Add("Tool");
			if (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1) types.Add("Armor");
			if (item.createTile != -1) {
				types.Add("Tile");
				if (TileID.Sets.Torch[item.createTile]) {
					types.Add("Torch");
				}
			}
			if (item.expert) types.Add("Expert");
			if (item.master) types.Add("Master");
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			data.Add("Types", types);

			data.AppendStat("PickPower", item.pick, 0);
			data.AppendStat("AxePower", item.axe, 0);
			data.AppendStat("HammerPower", item.hammer, 0);
			data.AppendStat("FishPower", item.fishingPole, 0);
			data.AppendStat("BaitPower", item.bait, 0);

			if (item.createTile != -1) {
				ModTile tile = TileLoader.GetTile(item.createTile);
				if (tile is not null) {
					data.AppendStat(Main.tileHammer[item.createTile] ? "HammerReq" : "PickReq", tile.MinPick, 0);
				}
				int width = 1, height = 1;
				if (TileObjectData.GetTileData(item.createTile, item.placeStyle) is TileObjectData tileData) {
					width = tileData.Width;
					height = tileData.Height;
				}
				data.Add("PlacementSize", new JArray(width, height));
			}
			data.AppendStat("Defense", item.defense, 0);
			if (item.headSlot != -1) {
				data.Add("ArmorSlot", "Helmet");
			} else if (item.bodySlot != -1) {
				data.Add("ArmorSlot", "Shirt");
			} else if (item.legSlot != -1) {
				data.Add("ArmorSlot", "Pants");
			}
			data.AppendStat("ManaCost", item.mana, 0);
			data.AppendStat("HealLife", item.healLife, 0);
			data.AppendStat("HealMana", item.healMana, 0);
			data.AppendStat("Damage", item.damage, -1);
			data.AppendStat("ArmorPenetration", item.ArmorPenetration, 0);
			if (item.damage > 0) {
				string damageClass = item.DamageType.DisplayName.Value;
				damageClass = damageClass.Replace(" damage", "").Trim();
				damageClass = Regex.Replace(damageClass, "( |^)(\\w)", (match) => match.Groups[1].Value + match.Groups[2].Value.ToUpper());
				data.Add("DamageClass", damageClass);
			}
			data.AppendStat("Knockback", item.knockBack, 0);
			data.AppendStat("Crit", item.crit + 4, 4);
			data.AppendStat("UseTime", item.useTime, 100);
			data.AppendStat("Velocity", item.shootSpeed, 0);
			if (item.ToolTip.Lines > 1) {
				JArray itemTooltip = [];
				for (int i = 0; i < item.ToolTip.Lines; i++) {
					string line = item.ToolTip.GetLine(i);
					if (!string.IsNullOrWhiteSpace(line)) itemTooltip.Add(line);
				}
				data.AppendJStat("Tooltip", itemTooltip, new());
			} else if (item.ToolTip.Lines > 0) {
				data.AppendStat("Tooltip", item.ToolTip.GetLine(0), string.Empty);
			}
			data.AppendStat("Rarity", WikiPageExporter.GetWikiItemRarity(item), "");
			if (customStat?.Buyable ?? false) data.AppendStat("Buy", item.value, 0);
			data.AppendStat("Sell", item.value / 5, 0);
			data.AppendJStat("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForItemID(item.type).GetDropRates()), []);
			{
				string baseKey = $"WikiGenerator.Stats.{modItem.Mod?.Name}.{modItem.Name}.";
				string key = baseKey + "Source";
				if (Language.Exists(key)) data.AppendStat("Source", Language.GetTextValue(key), key);

				key = baseKey + "Effect";
				if (Language.Exists(key)) data.AppendStat("Effect", Language.GetTextValue(key), key);

			}
			if (customStat is not null) {
				string baseKey = $"WikiGenerator.Stats.{modItem.Mod?.Name}.{modItem.Name}.";
				foreach (string stat in customStat.LocalizedStats) {
					data.AppendStat(stat, Language.GetOrRegister(baseKey + stat).Value, "");
				}
				customStat?.ModifyWikiStats(data);
			}
			data.AppendStat("SpriteWidth", item.ModItem is null ? item.width : ModContent.Request<Texture2D>(item.ModItem.Texture).Width(), 0);
			data.AppendStat("InternalName", item.ModItem?.Name, null);
			yield return (PageName(modItem), data);
		}
		public override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(ModItem value) {
			if (Main.itemAnimations[value.Type] is DrawAnimation animation) {
				yield return (WikiPageExporter.GetWikiName(value), SpriteGenerator.GenerateAnimationSprite(TextureAssets.Item[value.Type].Value, animation));
			}
			yield break;
		}
	}
	public interface IWikiNPC {
		Rectangle DrawRect { get; }
		int AnimationFrames { get; }
	}
	public class NPCWikiProvider : WikiProvider<ModNPC> {
		public override string PageName(ModNPC modNPC) => WikiPageExporter.GetWikiName(modNPC);
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			Dictionary<string, object> context = new() {
				["Name"] = WikiPageExporter.GetWikiName(modNPC),
				["DisplayName"] = Lang.GetNPCNameValue(npc.type)
			};

			IEnumerable<(string name, LocalizedText text)> pageTexts;
			if (modNPC is ICustomWikiStat wikiStats) {
				context["PageTextMain"] = wikiStats.PageTextMain.Value;
				pageTexts = wikiStats.PageTexts;
			} else {
				string key = $"WikiGenerator.{modNPC.Mod?.Name}.{modNPC.LocalizationCategory}.{context["Name"]}.MainText";
				context["PageTextMain"] = Language.GetOrRegister(key).Value;
				pageTexts = WikiPageExporter.GetDefaultPageTexts(modNPC);
			}
			foreach (var text in pageTexts) {
				context[text.name] = text.text;
			}
			return (WikiTemplate, context);
		}
		public override IEnumerable<(string, JObject)> GetStats(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			JObject data = [];
			ICustomWikiStat customStat = modNPC as ICustomWikiStat;
			data["Image"] = customStat?.CustomSpritePath ?? ("Images/" + WikiPageExporter.GetWikiName(modNPC));
			data["Name"] = npc.TypeName;
			JArray types = new("NPC");
			if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) types.Add("Boss");
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			data.Add("Types", types);

			bool getGoodWorld = Main.getGoodWorld;
			Main.getGoodWorld = false;
			int gameMode = Main.GameMode;
			JObject expertData = [];
			JObject masterData = [];
			try {
				Main.GameMode = GameModeID.Normal;
				npc.SetDefaults(npc.netID);
				data.AppendStat("MaxLife", npc.lifeMax, 0);
				data.AppendStat("Defense", npc.defense, 0);
				data.AppendStat("KBResist", 1 - npc.knockBackResist, 0);
				data.AppendJStat("Immunities", npc.GetImmunities(), []);
				data.AppendStat("Coins", npc.value, 0);
				data.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));

				Main.GameMode = GameModeID.Expert;
				npc.SetDefaults(npc.netID);
				expertData.AppendAltStat(data, "MaxLife", npc.lifeMax);
				expertData.AppendAltStat(data, "Defense", npc.defense);
				expertData.AppendAltStat(data, "KBResist", 1 - npc.knockBackResist);
				expertData.AppendAltStat(data, "Immunities", npc.GetImmunities());
				expertData.AppendAltStat(data, "Coins", npc.value);
				expertData.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));

				Main.GameMode = GameModeID.Master;
				npc.SetDefaults(npc.netID);
				masterData.AppendAltStat(data, "MaxLife", npc.lifeMax);
				masterData.AppendAltStat(data, "Defense", npc.defense);
				masterData.AppendAltStat(data, "KBResist", 1 - npc.knockBackResist);
				masterData.AppendAltStat(data, "Immunities", npc.GetImmunities());
				masterData.AppendAltStat(data, "Coins", npc.value);
				masterData.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));
			} finally {
				Main.GameMode = gameMode;
				Main.getGoodWorld = getGoodWorld;
				npc.SetDefaults(npc.netID);
			}
			for (int i = 0; i < BiomeNPCGlobals.assimilationProviders.Count; i++) {
				if (npc.TryGetGlobalNPC((GlobalNPC)BiomeNPCGlobals.assimilationProviders[i], out GlobalNPC gNPC) && gNPC is IAssimilationProvider assimilationProvider) {
					AssimilationAmount amount = assimilationProvider.GetAssimilationAmount(npc);
					if (amount != default) {
						string assName = assimilationProvider.AssimilationName;
						if (amount.Function is not null) {
							data.Add(assName, "variable");
							continue;
						}
						data.Add(assName, amount.ClassicAmount);
						if (amount.ExpertAmount.HasValue) expertData.Add(assName, amount.ExpertAmount.Value);
						if (amount.MasterAmount.HasValue) masterData.Add(assName, amount.MasterAmount.Value);
					}
				}
			}
			data.AppendJStat("Expert", expertData, []);
			data.AppendJStat("Master", masterData, []);

			customStat?.ModifyWikiStats(data);
			data.AppendStat("SpriteWidth", modNPC is null ? npc.width : ModContent.Request<Texture2D>(modNPC.Texture).Width(), 0);
			data.AppendStat("InternalName", modNPC?.Name, null);
			yield return (PageName(modNPC), data);
		}
		public override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(ModNPC modNPC) {
			if (modNPC is not IWikiNPC wikiNPC) yield break;
			yield return (WikiPageExporter.GetWikiName(modNPC), SpriteGenerator.GenerateAnimationSprite(modNPC.NPC, wikiNPC.DrawRect, wikiNPC.AnimationFrames));
		}
	}
	public interface INoSeperateWikiPage { }
	public class StatOnlyItemWikiProvider : ItemWikiProvider {
		protected override void Register() {
			base.Register();
			WikiPageExporter.TypedDataProviders.Add((typeof(INoSeperateWikiPage), this));
			WikiPageExporter.InterfaceReplacesGenericClassProvider.Add(typeof(INoSeperateWikiPage));
		}
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(ModItem modItem) => default;
	}
	public abstract class WikiProvider : ModType {
		protected override void Register() {
			ModTypeLookup<WikiProvider>.Register(this);
		}
		public abstract string PageName(object value);
		public virtual (PageTemplate template, Dictionary<string, object> context) GetPage(object value) => default;
		public virtual IEnumerable<(string name, JObject stats)> GetStats(object value) => default;
		public virtual IEnumerable<(string name, Texture2D texture)> GetSprites(object value) => default;
		public virtual IEnumerable<(string name, (Texture2D texture, int frames)[] texture)> GetAnimatedSprites(object value) => default;
	}
	public abstract class WikiProvider<T> : WikiProvider {
		protected override void Register() {
			base.Register();
			Type ownType = this.GetType();
			if (ownType.BaseType.IsGenericType) {
				Type baseWithGenerics = typeof(WikiProvider<>).MakeGenericType(ownType.BaseType.GenericTypeArguments);
				if (ownType.BaseType == baseWithGenerics) {
					WikiPageExporter.TypedDataProviders.Add((typeof(T), this));
				}
			}
		}
		public sealed override string PageName(object value) => value is T v ? PageName(v) : null;
		public sealed override (PageTemplate template, Dictionary<string, object> context) GetPage(object value) => value is T v ? GetPage(v) : default;
		public sealed override IEnumerable<(string, JObject)> GetStats(object value) => value is T v ? GetStats(v) : null;
		public sealed override IEnumerable<(string, Texture2D)> GetSprites(object value) => value is T v ? GetSprites(v) : null;
		public sealed override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(object value) => value is T v ? GetAnimatedSprites(v) : null;
		public abstract string PageName(T value);
		public abstract (PageTemplate template, Dictionary<string, object> context) GetPage(T value);
		public abstract IEnumerable<(string, JObject)> GetStats(T value);
		public virtual IEnumerable<(string, Texture2D)> GetSprites(T value) => default;
		public virtual IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(T value) => default;
	}
	public static class WikiExtensions {
		public static void AppendStat<T>(this JObject data, string name, T value, T defaultValue) {
			if (!value.Equals(defaultValue)) {
				data.Add(name, JToken.FromObject(value));
			}
		}
		public static void AppendJStat<T>(this JObject data, string name, T value, T defaultValue) where T : JToken {
			if (!JToken.DeepEquals(value, defaultValue)) {
				data.Add(name, JToken.FromObject(value));
			}
		}
		public static void AppendAltStat<T>(this JObject data, JObject normalData, string name, T value) {
			JToken jValue = (value as JToken) ?? new JValue(value);
			if (!JToken.DeepEquals(jValue, normalData.GetValue(name))) {
				data.Add(name, JToken.FromObject(value));
			}
		}
		public static List<DropRateInfo> GetDropRates(this List<IItemDropRule> rules, DropRateInfoChainFeed? ratesInfo = null) {
			List<DropRateInfo> ruleList = [];
			ratesInfo ??= new(1f);
			for (int j = 0; j < rules.Count; j++) {
				rules[j].ReportDroprates(ruleList, ratesInfo.Value);
			}
			return ruleList;
		}
		public static JArray FillWithLoot(this JArray self, List<DropRateInfo> loot, bool doRecursion = false) {
			for (int i = 0; i < loot.Count; i++) {
				DropRateInfo info = loot[i];
				for (int j = 0; j < (info.conditions?.Count ?? 0); j++) {
					if (!info.conditions[j].CanShowItemDropInUI()) goto skip;
				}
				Item item = ContentSamples.ItemsByType[info.itemId];
				JObject drop = new() {
					["Name"] = item.Name
				};
				if (info.dropRate != 1) drop.Add("Chance", $"{(info.dropRate * 100):0.###}%");
				if (info.stackMin != info.stackMax) {
					drop.Add("Amount", $"{info.stackMin}‒{info.stackMax}");
				} else if (info.stackMin != 1 || info.stackMax != 1) {
					drop.Add("Amount", info.stackMin);
				}
				if (item.ModItem?.Mod is not Origins) {
					drop.Add("LinkOverride", "https://terraria.wiki.gg/wiki/" + item.Name.Replace(' ', '_'));
					drop.Add("ImageOverride", "");
				}
				self.Add(drop);
				if (doRecursion) {
					List<DropRateInfo> itemLoot = Main.ItemDropsDB.GetRulesForItemID(info.itemId).GetDropRates(new(info.dropRate));
					if (itemLoot.Count != 0) {
						self.FillWithLoot(itemLoot);
					}
				}
				skip:;
			}
			return self;
		}
		public static string GetItemText(Item item, string note = "") {
			string name = item.Name;
			string text = "";
			if (item.ModItem?.Mod is Origins) {
				text = $"<a is=a-link image=$fromStats>{name}{(string.IsNullOrWhiteSpace(note) ? "" : $"<note>{note}</note>")}</a>";
			} else if (WikiPageExporter.LinkFormatters.TryGetValue(item.ModItem?.Mod, out var formatter)) {
				text = $"`{formatter(item.Name)}`";
				goto formatted;
			} else {
				text = item.Name;
			}
			formatted:
			if (item.stack != 1) text += $" ({item.stack})";
			return text;
		}
		public static string GetBuffText(int type) {
			string name = Lang.GetBuffName(type);
			string href = "";
			string image = "";
			if (BuffLoader.GetBuff(type)?.Mod is Origins) {
				image = " image=$fromStats";
			} else {
				href = " href=https://terraria.wiki.gg/wiki/" + name.Replace(' ', '_');
			}
			return $"<a is=a-link{href}{image}>{name}</a>";
		}
		public static JArray GetImmunities(this NPC npc) {
			JArray immunities = [];
			for (int i = 0; i < npc.buffImmune.Length; i++) {
				if (npc.buffImmune[i]) immunities.Add(GetBuffText(i));
			}
			return immunities;
		}
		public static (List<Recipe> recipes, List<Recipe> usedIn) GetRecipes(Item item) {
			List<Recipe> recipes = new();
			List<Recipe> usedIn = new();
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				if (recipe.HasResult(item.type)) {
					recipes.Add(recipe);
				}
				if (recipe.HasIngredient(item.type)) {
					usedIn.Add(recipe);
				} else {
					foreach (int num in recipe.acceptedGroups) {
						if (RecipeGroup.recipeGroups[num].ContainsItem(item.type)) {
							usedIn.Add(recipe);
						}
					}
				}
			}
			return (recipes, usedIn);
		}
		public enum RecipeUseType {
			NONE,
			RESULT,
			INGREDIENT
		}
		public static (List<Recipe> recipes, List<Recipe> usedIn) GetRecipes(Func<Recipe, RecipeUseType> condition) {
			List<Recipe> recipes = new();
			List<Recipe> usedIn = new();
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				switch (condition(recipe)) {
					case RecipeUseType.NONE:
					break;
					case RecipeUseType.RESULT:
					recipes.Add(recipe);
					break;
					case RecipeUseType.INGREDIENT:
					usedIn.Add(recipe);
					break;
				}
			}
			return (recipes, usedIn);
		}
		public static Func<Recipe, RecipeUseType> GetRecipeAllItemCondition(Func<Item, bool> condition) {
			return r => {
				if (condition(r.createItem)) return RecipeUseType.RESULT;
				for (int i = 0; i < r.requiredItem.Count; i++) {
					if (condition(r.requiredItem[i])) return RecipeUseType.INGREDIENT;
				}
				return RecipeUseType.NONE;
			};
		}
	}
}
