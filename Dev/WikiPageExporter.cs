using Microsoft.CodeAnalysis.Rename;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origins.Reflection;
using Origins.UI;
using PegasusLib.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static Origins.Dev.WikiPageExporter;

namespace Origins.Dev {
	public class WikiPageExporter : ILoadable {
		public static Dictionary<Type, Func<AbstractNPCShop, IEnumerable<AbstractNPCShop.Entry>>> ShopTypes { get; private set; } = [];
		public delegate string WikiLinkFormatter(string name, string note, bool imageOnly, bool canSpace = false);
		public static DictionaryWithNull<Mod, WikiLinkFormatter> LinkFormatters { get; private set; } = [];
		static List<(Type, WikiProvider)> typedDataProviders;
		public static List<(Type type, WikiProvider provider)> TypedDataProviders => typedDataProviders ??= [];
		public delegate bool ConditionalWikiProvider(object obj, out WikiProvider provider, ref bool replaceGenericClassProvider);
		static List<ConditionalWikiProvider> conditionalDataProviders;
		public static List<ConditionalWikiProvider> ConditionalDataProviders => conditionalDataProviders ??= [];
		static HashSet<Type> interfaceReplacesGenericClassProvider;
		public static HashSet<Type> InterfaceReplacesGenericClassProvider => interfaceReplacesGenericClassProvider ??= [];
		public static Dictionary<int, LocalizedText> requiredTileWikiTextOverride = [];
		public static Dictionary<Condition, LocalizedText> recipeConditionWikiTextOverride = [];
		static Dictionary<Type, Func<TextSnippet, string>> chatTagSanitizers;
		public static Dictionary<Type, Func<TextSnippet, string>> ChatTagSanitizers => chatTagSanitizers ??= new() {
			[typeof(Wiggle_Handler.Wiggle_Snippet)] = snippet => $"<a-wiggle>{snippet.Text}</a-wiggle>",
			[typeof(Italics_Snippet_Handler.Italics_Snippet)] = snippet => $"<i>{snippet.Text}</i>",
			[typeof(Buff_Hint_Snippet)] = _snippet => {
				if (_snippet is not Buff_Hint_Snippet snippet || snippet.buffType == -1) return _snippet.Text;
				if (snippet.buff is null) return $"<a href=\"https://terraria.wiki.gg/wiki/{Lang.GetBuffName(snippet.buffType).Replace(' ', '_')}\">{Lang.GetBuffName(snippet.buffType)}</a>";
				if (snippet.buff.Mod is Origins) return $"<a href=\"{GetWikiName(snippet.buff)}\">{ProcessTags(Lang.GetBuffName(snippet.buffType))}</a>";
				return Lang.GetBuffName(snippet.buffType);
			}
		};
		public void Load(Mod mod) {
			ShopTypes.Add(typeof(TravellingMerchantShop), shop => ((TravellingMerchantShop)shop).ActiveEntries);
			ShopTypes.Add(typeof(NPCShop), shop => ((NPCShop)shop).Entries);
			LinkFormatters[Origins.instance] = (t, note, imageOnly, _) => {
				string formattedName = t.Replace(" ", "_");
				return $"<a is=a-link image=$fromStats{(imageOnly ? " imageOnly" : "")}>{t}{(string.IsNullOrWhiteSpace(note) ? "" : $"<note>{note}</note>")}</a>";
			};
			LinkFormatters[null] = (t, note, imageOnly, canSpace) => {
				string formattedName = t.Replace(" ", "_");
				return $"{(imageOnly && canSpace ? " " : "")}<a is=a-link href=\"https://terraria.wiki.gg/wiki/{formattedName}\">{t}</a>";
			};
			requiredTileWikiTextOverride[TileID.Bottles] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.Bottle");
			requiredTileWikiTextOverride[TileID.Anvils] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.Anvils");
			requiredTileWikiTextOverride[TileID.MythrilAnvil] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.MythrilAnvil");
			requiredTileWikiTextOverride[TileID.WorkBenches] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.WorkBenches");
			recipeConditionWikiTextOverride[Condition.InGraveyard] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.EctoMist");
			recipeConditionWikiTextOverride[RecipeConditions.ShimmerTransmutation] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.ShimmerTransmutation");
			recipeConditionWikiTextOverride[RecipeConditions.RivenWater] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.RivenWater");
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
		readonly static ConstructorInfo RecipeCtor = typeof(Recipe).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, [typeof(Mod)]);
		internal static Recipe CreateFakeRecipe(int result) {
			Recipe recipe = (Recipe)RecipeCtor.Invoke([Origins.instance]);
			recipe.ReplaceResult(result);
			return recipe;
		}
		static void TryUpdatingExistingPage(string pagePath, PageTemplate template, Dictionary<string, object> context) {
			try {
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
			} catch (Exception error) {
				Origins.instance.Logger.Error($"Encountered error while parsing {pagePath}:", error);
			}
		}
		public static void ExportItemPage(Item item) {
			foreach (WikiProvider provider in GetWikiProviders(item.ModItem)) {
				ICustomWikiStat customWikiStat = item.ModItem as ICustomWikiStat;
				string pagePath = GetWikiPagePath(provider.PageName(item.ModItem));
				(PageTemplate template, Dictionary<string, object> context) = provider.GetPage(item.ModItem);
				if (context == null) continue;
				if ((customWikiStat?.FullyGeneratable ?? false) || !File.Exists(pagePath)) {
					WriteFileNoUnneededRewrites(pagePath, template.Resolve(context));
				} else {
					TryUpdatingExistingPage(pagePath, template, context);
				}
			}
		}
		public static void ExportItemStats(Item item) {
			if (item.ModItem is ICustomWikiStat customStats && !customStats.CanExportStats) return;
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
					WriteFileNoUnneededRewrites(GetWikiPagePath((npc.ModNPC as ICustomWikiStat)?.CustomStatPath ?? provider.PageName(npc.ModNPC)), template.Resolve(context));
				}
			}
		}
		public static void ExportNPCStats(NPC npc) {
			if (npc.ModNPC is ICustomWikiStat customStats && !customStats.CanExportStats) return;
			foreach (WikiProvider provider in GetWikiProviders(npc.ModNPC)) {
				foreach ((string name, JObject stats) stats in provider.GetStats(npc.ModNPC)) {
					WriteFileNoUnneededRewrites(
						GetWikiStatPath(stats.name),
						JsonConvert.SerializeObject(stats.stats, Formatting.Indented)
					);
				}
			}
		}
		public static void ExportBuffStats(ModBuff buff) {
			if (buff is ICustomWikiStat customStats && !customStats.CanExportStats) return;
			foreach (WikiProvider provider in GetWikiProviders(buff)) {
				foreach ((string name, JObject stats) stats in provider.GetStats(buff)) {
					WriteFileNoUnneededRewrites(
						GetWikiStatPath(stats.name),
						JsonConvert.SerializeObject(stats.stats, Formatting.Indented)
					);
				}
			}
		}
		public static void ExportContentSprites(object content) {
			int screenWidth = Main.screenWidth;
			int screenHeight = Main.screenHeight;
			foreach (WikiProvider provider in GetWikiProviders(content)) {
				foreach ((string name, Texture2D texture) in provider.GetSprites(content) ?? Array.Empty<(string, Texture2D)>()) {
					WikiImageExporter.ExportImage(name, texture);
				}
				foreach ((string name, (Texture2D texture, int frames)[] textures) in provider.GetAnimatedSprites(content) ?? Array.Empty<(string, (Texture2D texture, int frames)[])>()) {
					WikiImageExporter.ExportAnimatedImage(name, textures);
				}
			}
			Main.screenWidth = screenWidth;
			Main.screenHeight = screenHeight;
		}
		public static string GetWikiName(ModItem modItem) => SanitizeWikiName(modItem.DisplayName.Value);
		public static string GetWikiName(ModNPC modNPC) => SanitizeWikiName(modNPC.DisplayName.Value);
		public static string GetWikiName(ModBuff modBuff) => SanitizeWikiName(Lang.GetBuffName(modBuff.Type));
		public static string SanitizeWikiName(string name) => WebUtility.UrlEncode(string.Concat(ChatManager.ParseMessage(name, default).Select(s => s.Text)).Replace(' ', '_')).Replace("%27", "'").Replace("\"", "%22");
		public static string GetWikiPagePath(string name) => Path.Combine(DebugConfig.Instance.WikiPagePath, name + ".html");
		public static string GetWikiStatPath(string name) => Path.Combine(DebugConfig.Instance.StatJSONPath, name + ".json");
		public static string GetWikiItemImagePath(ModItem modItem) => Main.itemAnimations[modItem.Type] is not null ? modItem.Name.Replace(' ', '_') : modItem.Texture.Replace(modItem.Mod.Name, "§ModImage§");
		public static string GetWikiImagePath(string path) => string.Join('/', "§ModImage§", path);
		public static string GetWikiItemRarity(Item item) => (RarityLoader.GetRarity(item.rare)?.Name ?? ItemRarityID.Search.GetName(item.rare)).Replace("Rarity", "");
		public void Unload() {
			wikiTemplate = null;
			LinkFormatters = null;
			typedDataProviders = null;
			conditionalDataProviders = null;
			interfaceReplacesGenericClassProvider = null;
			requiredTileWikiTextOverride = null;
			recipeConditionWikiTextOverride = null;
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
		public static string ProcessTags(string text) {
			List<TextSnippet> snippets = ChatManager.ParseMessage(text, Color.White);
			StringBuilder builder = new();
			for (int i = 0; i < snippets.Count; i++) {
				if (ChatTagSanitizers.TryGetValue(snippets[i].GetType(), out Func<TextSnippet, string> sanitizer)) {
					builder.Append(sanitizer(snippets[i]));
				} else {
					builder.Append(snippets[i].Text);
				}
			}
			return builder.ToString();
		}
		public static void AddTorchLightStats(JObject data, Vector3 light) {
			float intensity = MathF.Max(MathF.Max(light.X, light.Y), light.Z);
			if (intensity > 1) light /= intensity;
			data.Add("LightIntensity", $"{intensity * 100:0.#}%");
			data.Add("LightColor", new JArray() { light.X, light.Y, light.Z });
		}
		public static bool TryGetLinkFormat(Mod mod, ICustomLinkFormat customFormat, out WikiLinkFormatter format) {
			if (customFormat is not null && customFormat.CustomFormatter is not null) {
				format = customFormat.CustomFormatter;
				return true;
			} 
			if (LinkFormatters.TryGetValue(mod, out format)) {
				return true;
			}
			return false;
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
						while (i < text.Length && !char.IsWhiteSpace(text[i])) currentText.Append(text[i++]);
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
									while (parsePos < text.Length && !char.IsWhiteSpace(text[parsePos])) directiveParser.Append(text[parsePos++]);
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
					foreach (object item in enumerable) {
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
					if (context.TryGetValue("Name", out object wikiName)) {
						string usedIn = name == "UsedIn" ? " usedIn" : "";
						builder.Append($"<a-recipes src=\"{wikiName}\"{usedIn}></a-recipes>");
						return builder.ToString();
					}				
					builder.AppendLine("<a-recipes>");
					bool firstStation = true;
					foreach (IGrouping<RecipeRequirements, Recipe> group in recipes.GroupBy((r) => new RecipeRequirements(r))) {
						if (!firstStation) builder.Append(',');
						firstStation = false;
						builder.AppendLine("{");
						if (group.Key.requirements.Length > 0) {
							builder.AppendLine("stations:[");
							bool firstReq = true;
							foreach (RecipeRequirement requirement in group.Key.requirements) {
								if (string.IsNullOrEmpty(requirement.ToString())) continue;
								if (!firstReq) builder.Append(',');
								firstReq = false;
								builder.AppendLine($"`{requirement}`");
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
		public readonly RecipeRequirement[] requirements =
				recipe.requiredTile.DefaultIfEmpty(-1).Select(t => new TileRecipeRequirement(t))
				.Concat(recipe.Conditions.Select(c => (RecipeRequirement)new ConditionRecipeRequirement(c))
				).ToArray();

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
	public abstract record class RecipeRequirement {
		public abstract override string ToString();
		public virtual string ToStringCompact() => ToString();
	}
	public record TileRecipeRequirement(int Tile) : RecipeRequirement {
		public override string ToString() {
			if (Main.dedServ) return string.Empty;
			if (requiredTileWikiTextOverride.TryGetValue(Tile, out LocalizedText text)) return text.Value;
			Mod reqMod = TileLoader.GetTile(Tile)?.Mod;
			string reqName = Tile == -1 ? "By Hand" : Lang.GetMapObjectName(MapHelper.TileToLookup(Tile, 0));
			if (LinkFormatters.TryGetValue(reqMod, out WikiLinkFormatter formatter)) {
				return formatter(reqName, null, false);
			} else {
				Origins.instance.Logger.Warn($"No wiki link formatter for mod {reqMod}, skipping requirement for {reqName}");
			}
			return string.Empty;
		}
		public override string ToStringCompact() {
			if (requiredTileWikiTextOverride.TryGetValue(Tile, out LocalizedText text)) {
				string key = text.Key + ".Compact";
				return Language.Exists(key) ? Language.GetTextValue(key) : ToString();
			}
			return ToString();
		}
	}
	public record ConditionRecipeRequirement(Condition Condition) : RecipeRequirement {
		public override string ToString() => recipeConditionWikiTextOverride.TryGetValue(Condition, out LocalizedText text) ? text.Value : Condition.Description.Value;
		public override string ToStringCompact() {
			if (recipeConditionWikiTextOverride.TryGetValue(Condition, out LocalizedText text)) {
				string key = text.Key + ".Compact";
				return Language.Exists(key) ? Language.GetTextValue(key) : ToString();
			}
			return ToString();
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
	public record LinkInfo (string Name, string Href = null, string Image = null, string Note = null) {
		public static string FromStats = "$fromStats";
		public WikiLinkFormatter Formatter() => (nameIn, noteIn, imageOnlyIn, canSpaceIn) => {
			StringBuilder builder = new();
			string formattedName = Name.Replace(" ", "_");
			static string Attribute(string name, string value) => value is not null ? $"{name}={value}" : "";
			static string EmptyAttribute(string name, bool condition) => condition ? name : "";
			builder.Append('<');
			string attributes = string.Join(' ',
				new string[] { EmptyAttribute("a", true),
				Attribute("is", "a-link"),
				Attribute("href", Href),
				Attribute("image", Image),
				EmptyAttribute("imageOnly", imageOnlyIn) }
				.Where(s => !string.IsNullOrEmpty(s)));
			builder.Append(attributes);
			builder.Append('>');
			builder.Append(Name);
			builder.Append(string.IsNullOrWhiteSpace(Note) ? "" : $"<note>{Note}</note>");
			builder.Append("</a>");
			return builder.ToString();
		};
	}
	public interface ICustomLinkFormat {
		WikiLinkFormatter CustomFormatter => null;
	}
	public interface ICustomWikiStat {
		bool Buyable => false;
		void ModifyWikiStats(JObject data) { }
		string[] Categories => [];
		bool? Hardmode => null;
		bool FullyGeneratable => false;
		bool ShouldHavePage => true;
		bool NeedsCustomSprite => false;
		string CustomSpritePath => null;
		string CustomStatPath => null;
		bool CanExportStats => true;
		LocalizedText PageTextMain => (this is ILocalizedModType modType) ? WikiPageExporter.GetDefaultMainPageText(modType) : null;
		IEnumerable<(string name, LocalizedText text)> PageTexts => (this is ILocalizedModType modType) ? WikiPageExporter.GetDefaultPageTexts(modType) : null;
		IEnumerable<WikiProvider> GetWikiProviders() => WikiPageExporter.GetDefaultProviders(this);
		IEnumerable<string> LocalizedStats => [];
	}
	public interface IWikiNPC {
		Rectangle DrawRect { get; }
		int AnimationFrames { get; }
		int FrameDuration => 1;
		NPCExportType ImageExportType => NPCExportType.Bestiary;
		Range FrameRange => new(0, AnimationFrames);
	}
	public enum NPCExportType {
		Bestiary,
		SpriteSheet,
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
				try {
					rules[j].ReportDroprates(ruleList, ratesInfo.Value);
				} catch (Exception) { }
			}
			return ruleList;
		}
		public static void GetAllDropRates(this List<IItemDropRule> rules, out List<DropRateInfo> classic, out List<DropRateInfo> expert, out List<DropRateInfo> master, DropRateInfoChainFeed? ratesInfo = null) {
			classic = [];
			expert = [];
			master = [];
			ratesInfo ??= new(1f);
			int gameMode = Main.GameMode;
			try {
				Main.GameMode = GameModeID.Normal;
				for (int j = 0; j < rules.Count; j++) {
					try {
						rules[j].ReportDroprates(classic, ratesInfo.Value);
					} catch (Exception) { }
				}
				Main.GameMode = GameModeID.Expert;
				for (int j = 0; j < rules.Count; j++) {
					try {
						rules[j].ReportDroprates(expert, ratesInfo.Value);
					} catch (Exception) { }
				}
				Main.GameMode = GameModeID.Master;
				for (int j = 0; j < rules.Count; j++) {
					try {
						rules[j].ReportDroprates(master, ratesInfo.Value);
					} catch (Exception) { }
				}
			} finally {
				Main.GameMode = gameMode;
			}
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
		public static string GetItemText(Item item, string note = "", bool imageOnly = false) {
			string text = "";
			int recipeGroup = item.GetGlobalItem<RecipeGroupTrackerGlobalItem>().recipeGroup;
			bool useRecipeGroup = recipeGroup != -1;
			if (useRecipeGroup) {
				string internalName = RecipeGroup.recipeGroups[recipeGroup].GetInternalName();
				if (internalName is null || (internalName.Contains(':') && !internalName.StartsWith("Origins:"))) useRecipeGroup = false;
			}
			if (useRecipeGroup) {
				text = $"<a is=a-link image=\"RecipeGroups/{RecipeGroupPage.GetRecipeGroupWikiName(recipeGroup)}\" href=Recipe_Groups>{RecipeGroup.recipeGroups[recipeGroup].GetText()}</a>";
			} else if (WikiPageExporter.TryGetLinkFormat(item.ModItem?.Mod, item.ModItem as ICustomLinkFormat, out WikiLinkFormatter formatter)) {
				text = $"{formatter(item.Name, note, imageOnly, item.stack > 1)}";
				goto formatted;
			} else {
				text = item.Name;
			}
			formatted:
			if (item.stack != 1) text += $" ({item.stack})";
			return text;
		}
		public static string GetNPCText(NPC npc, string note = "", bool imageOnly = false) {
			string text = "";
			if (WikiPageExporter.LinkFormatters.TryGetValue(npc.ModNPC?.Mod, out WikiLinkFormatter formatter)) {
				text = $"{formatter(npc.TypeName, note, imageOnly)}";
			} else {
				text = npc.TypeName;
			}
			return text;
		}
		public static string GetBuffText(int type) {
			string name = Lang.GetBuffName(type);
			string href = "";
			string image = "";
			if (BuffLoader.GetBuff(type)?.Mod is Origins) {
				image = " image=$fromStats";
			} else if (type < BuffID.Count) {
				href = " href=https://terraria.wiki.gg/wiki/" + name.Replace(' ', '_');
			}
			return $"<a is=a-link{href}{image}>{name}</a>";
		}
		public static JArray GetImmunities(this NPC npc) {
			if (NPCID.Sets.ImmuneToAllBuffs[npc.type]) {
				return [Language.GetOrRegister("WikiGenerator.Generic.ImmuneToAllBuffs").Value];
			}
			if (NPCID.Sets.ImmuneToRegularBuffs[npc.type]) {
				return [Language.GetOrRegister("WikiGenerator.Generic.ImmuneToNormalBuffs").Value];
			}
			JArray immunities = [];
			for (int i = 0; i < npc.buffImmune.Length; i++) {
				if (npc.buffImmune[i] && (i < BuffID.Count || ModContent.GetModBuff(i).Mod is Origins)) immunities.Add(GetBuffText(i));
			}
			return immunities;
		}
		public static JArray GetEnvironment(this NPC npc) {
			BestiaryEntry entry = Main.BestiaryDB.FindEntryByNPCID(npc.type);
			JArray environments = [];
			foreach (IBestiaryInfoElement info in entry.Info) {
				if (info is ModBiomeBestiaryInfoElement biomeInfo) {
					Mod mod = ModBestiaryInfoElementMethods._mod.GetValue(biomeInfo);
					AddEnvironment(mod, biomeInfo);
				} else if (info is FilterProviderInfoElement filter) {
					AddEnvironment(null, filter);
				}
			}
			void AddEnvironment(Mod mod, IFilterInfoProvider info) {
				string name = Language.GetTextValue(info.GetDisplayNameKey());
				if (LinkFormatters.TryGetValue(mod, out WikiLinkFormatter formatter)) {
					environments.Add(formatter(name, null, false));
				}
			}
			return environments;
		}
		public static string GetBestiaryText(this NPC npc) {
			BestiaryEntry entry = Main.BestiaryDB.FindEntryByNPCID(npc.type);
			foreach (IBestiaryInfoElement info in entry.Info) {
				if (info is FlavorTextBestiaryInfoElement quote) {
					string key = FlavorTextBestiaryInfoElementMethods._key?.GetValue(quote);
					return key != null && Language.Exists(key) ? Language.GetTextValue(key) : "";
				}
			}
			return "";
		}
		public static (List<Recipe> recipes, List<Recipe> usedIn) GetRecipes(Item item) {
			List<Recipe> recipes = [];
			List<Recipe> usedIn = [];
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				if (recipe.Disabled) continue;
				if (recipe.Mod is not null and not Origins) continue;
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
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				if (ItemID.Sets.ShimmerTransformToItem[i] == item.type) {
					recipes.Add(CreateFakeRecipe(item.type).AddIngredient(i).AddCondition(RecipeConditions.ShimmerTransmutation));
					break;
				}
			}
			if (ItemID.Sets.ShimmerTransformToItem[item.type] != -1) {
				usedIn.Add(CreateFakeRecipe(ItemID.Sets.ShimmerTransformToItem[item.type]).AddIngredient(item.type).AddCondition(RecipeConditions.ShimmerTransmutation));
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
