using Microsoft.CodeAnalysis.Rename;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.NPCs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
		public static Dictionary<int, LocalizedText> requiredTileWikiTextOverride = [];
		public static Dictionary<Condition, LocalizedText> recipeConditionWikiTextOverride = [];
		public void Load(Mod mod) {
			LinkFormatters[Origins.instance] = (t) => {
				string formattedName = t.Replace(" ", "_");
				return $"<a is=a-link image=$fromStats>{t}</a>";
			};
			LinkFormatters[null] = (t) => {
				string formattedName = t.Replace(" ", "_");
				return $"<a is=a-link href=\"https://terraria.wiki.gg/wiki/{formattedName}\">{t}</a>";
			};
			requiredTileWikiTextOverride[TileID.Bottles] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.Bottle");
			recipeConditionWikiTextOverride[Condition.InGraveyard] = Language.GetOrRegister("WikiGenerator.Generic.RecipeConditions.EctoMist");
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
		public static readonly Condition ShimmerTransmutationWikiCondition = new(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerTransmutation"), () => false);
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
					WriteFileNoUnneededRewrites(GetWikiPagePath(provider.PageName(npc.ModNPC)), template.Resolve(context));
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
		static UInt32[] CRCTable;
		static bool crc_table_computed = false;
		static string currentCRCText = "";
		static int currentCRCLength = 0;
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
		public static string SanitizeWikiName(string name) => WebUtility.UrlEncode(name.Replace(' ', '_')).Replace("%27", "'");
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
					bool firstStation = true;
					foreach (var group in recipes.GroupBy((r) => new RecipeRequirements(r))) {
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
				recipe.requiredTile.Select(t => new TileRecipeRequirement(t))
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
	}
	public record TileRecipeRequirement(int Tile) : RecipeRequirement {
		public override string ToString() {
			if (requiredTileWikiTextOverride.TryGetValue(Tile, out LocalizedText text)) return text.Value;
			Mod reqMod = TileLoader.GetTile(Tile)?.Mod;
			string reqName = Lang.GetMapObjectName(MapHelper.TileToLookup(Tile, 0));
			if (LinkFormatters.TryGetValue(reqMod, out WikiLinkFormatter formatter)) {
				return formatter(reqName);
			} else {
				Origins.instance.Logger.Warn($"No wiki link formatter for mod {reqMod}, skipping requirement for {reqName}");
			}
			return string.Empty;
		}
	}
	public record ConditionRecipeRequirement(Condition Condition) : RecipeRequirement {
		public override string ToString() => recipeConditionWikiTextOverride.TryGetValue(Condition, out LocalizedText text) ? text.Value : Condition.Description.Value;
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
		string[] Categories => [];
		bool? Hardmode => null;
		bool FullyGeneratable => false;
		bool ShouldHavePage => true;
		bool NeedsCustomSprite => false;
		string CustomSpritePath => null;
		bool CanExportStats => true;
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
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			if (item.pick != 0 || item.axe != 0 || item.hammer != 0 || item.fishingPole != 0 || item.bait != 0) types.Add("Tool");
			if (item.accessory) types.Add("Accessory");
			if (item.damage > 0 && item.useStyle != ItemUseStyleID.None && (!types.Any(t => t.ToString() == "Tool") || types.Any(t => t.ToString() == "ToolWeapon"))) {
				types.Add("Weapon");
				WeaponTypes weaponType = WeaponTypes.None;
				for (int i = 0; i < types.Count; i++) {
					if (Enum.TryParse(types[i].ToString(), out WeaponTypes type)) {
						weaponType = type;
						break;
					}
				}
				if (weaponType == WeaponTypes.None) {
					if (item.CountsAsClass(DamageClasses.Explosive)) {
						if (item.useAmmo == ModContent.ItemType<Resizable_Mine_One>()) {
							weaponType = WeaponTypes.CanisterLauncher;
						} else if (item.CountsAsClass(DamageClasses.ThrownExplosive)) {
							weaponType = WeaponTypes.ThrownExplosive;
						} else {
							weaponType = WeaponTypes.OtherExplosive;
						}
					}
					if (weaponType == WeaponTypes.None && item.shoot > ProjectileID.None) {
						switch (ContentSamples.ProjectilesByType[item.shoot].aiStyle) {
							case ProjAIStyleID.Boomerang:
							weaponType = WeaponTypes.Boomerang;
							break;

							case ProjAIStyleID.Yoyo:
							weaponType = WeaponTypes.Yoyo;
							break;
						}
						if (ItemID.Sets.Spears[item.type]) {
							weaponType = WeaponTypes.Spear;
						}

						if (item.CountsAsClass(DamageClass.Summon) && !types.Any(t => t.ToString() == "Incantation")) {
							if (Origins.ArtifactMinion[item.shoot]) weaponType = WeaponTypes.Artifact;
							if (ProjectileID.Sets.IsAWhip[item.shoot]) {
								weaponType = WeaponTypes.Whip;
							} else if (item.CountsAsClass<Incantation>()) {
								weaponType = WeaponTypes.Incantation;
							} else {
								Projectile proj = ContentSamples.ProjectilesByType[item.shoot];
								if (proj.minion) weaponType = WeaponTypes.Minion;
								else if (proj.sentry) weaponType = WeaponTypes.Sentry;
								else weaponType = WeaponTypes.OtherSummon;
							}
						}
					}
					if (weaponType == WeaponTypes.None && item.CountsAsClass(DamageClass.Magic)) {
						if (Item.staff[item.type]) weaponType = WeaponTypes.Wand;
						else weaponType = WeaponTypes.OtherMagic;
					}
					if (weaponType == WeaponTypes.None && item.CountsAsClass(DamageClass.Melee)) weaponType = WeaponTypes.OtherMelee;
					if (weaponType == WeaponTypes.None) {
						switch (item.useAmmo) {
							case ItemID.WoodenArrow:
							weaponType = WeaponTypes.Bow;
							break;
							case ItemID.MusketBall:
							weaponType = WeaponTypes.Gun;
							break;

							default:
							if (item.useAmmo == ModContent.ItemType<Metal_Slug>()) {
								weaponType = WeaponTypes.Handcannon;
							} else if (item.useAmmo == ModContent.ItemType<Harpoon>()) {
								weaponType = WeaponTypes.HarpoonGun;
							} else if (item.useAmmo == AmmoID.Rocket) {
								weaponType = WeaponTypes.RocketLauncher;
							} else if (item.useAmmo == ModContent.ItemType<Resizable_Mine_One>()) {
								weaponType = WeaponTypes.CanisterLauncher;
							}
							break;
						}
					}
					if (weaponType == WeaponTypes.None && item.CountsAsClass(DamageClass.Ranged)) {
						weaponType = WeaponTypes.OtherRanged;
					}
					if (weaponType == WeaponTypes.None && !item.noMelee && item.useStyle == ItemUseStyleID.Swing) weaponType = WeaponTypes.Sword;
				}
				if (weaponType != WeaponTypes.None && !types.Any(t => t.ToString() == weaponType.ToString())) types.Add(weaponType.ToString());
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
			if (customStat?.Hardmode ?? (!item.consumable && item.rare > ItemRarityID.Orange)) types.Add("Hardmode");

			if (ItemID.Sets.IsFood[item.type]) types.Add("Food");
			if (item.ammo != 0 && item.ammo != ItemID.CopperOre) types.Add("Ammo");
			if (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1) types.Add("Armor");
			if (item.createTile != -1) {
				types.Add("Tile");
				if (TileID.Sets.Torch[item.createTile]) {
					types.Add("Torch");
				}
			}
			if (item.expert) types.Add("Expert");
			if (item.master) types.Add("Master");
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
				data.AppendJStat("Tooltip", itemTooltip, []);
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
			if (item.makeNPC > 0 && NPCLoader.GetNPC(item.makeNPC) is ModNPC modNPC) {
				yield return (PageName(modItem) + "_NPC", NPCWikiProvider.GetNPCStats(modNPC));
			}
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
		public static JObject GetNPCStats(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			JObject data = [];
			ICustomWikiStat customStat = modNPC as ICustomWikiStat;
			data["Image"] = customStat?.CustomSpritePath ?? (WikiPageExporter.GetWikiName(modNPC));
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
			NPC tempInstance = new();
			bool dontDoHardmodeScaling = NPCID.Sets.DontDoHardmodeScaling[npc.type];
			try {
				NPCID.Sets.DontDoHardmodeScaling[npc.type] = true;
				Main.GameMode = GameModeID.Normal;
				tempInstance.SetDefaults(npc.netID, new NPCSpawnParams {
					gameModeData = Main.GameModeInfo,
					playerCountForMultiplayerDifficultyOverride = 1
				});
				data.AppendStat("MaxLife", tempInstance.lifeMax, 0);
				data.AppendStat("Defense", tempInstance.defense, 0);
				data.AppendStat("KBResist", 1 - tempInstance.knockBackResist, 0);
				data.AppendJStat("Immunities", tempInstance.GetImmunities(), []);
				data.AppendStat("Coins", tempInstance.value, 0);
				data.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));

				Main.GameMode = GameModeID.Expert;
				_ = Main.expertMode;
				tempInstance.SetDefaults(npc.netID, new NPCSpawnParams {
					gameModeData = Main.GameModeInfo,
					playerCountForMultiplayerDifficultyOverride = 1
				});
				expertData.AppendAltStat(data, "MaxLife", tempInstance.lifeMax);
				expertData.AppendAltStat(data, "Defense", tempInstance.defense);
				expertData.AppendAltStat(data, "KBResist", 1 - tempInstance.knockBackResist);
				expertData.AppendAltStat(data, "Immunities", tempInstance.GetImmunities());
				expertData.AppendAltStat(data, "Coins", tempInstance.value);
				expertData.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));

				Main.GameMode = GameModeID.Master;
				tempInstance.SetDefaults(npc.netID, new NPCSpawnParams {
					gameModeData = Main.GameModeInfo,
					playerCountForMultiplayerDifficultyOverride = 1
				});
				masterData.AppendAltStat(data, "MaxLife", tempInstance.lifeMax);
				masterData.AppendAltStat(data, "Defense", tempInstance.defense);
				masterData.AppendAltStat(data, "KBResist", 1 - tempInstance.knockBackResist);
				masterData.AppendAltStat(data, "Immunities", tempInstance.GetImmunities());
				masterData.AppendAltStat(data, "Coins", tempInstance.value);
				masterData.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));
			} finally {
				Main.GameMode = gameMode;
				Main.getGoodWorld = getGoodWorld;
				NPCID.Sets.DontDoHardmodeScaling[npc.type] = dontDoHardmodeScaling;
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
			if (!data.ContainsKey("SpriteWidth")) data.AppendStat("SpriteWidth", modNPC is null ? npc.width : ModContent.Request<Texture2D>(modNPC.Texture).Width(), 0);
			if (!data.ContainsKey("InternalName")) data.AppendStat("InternalName", modNPC?.Name, null);
			return data;
		}
		public override IEnumerable<(string, JObject)> GetStats(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			if (npc.catchItem > 0) yield break;
			string segmentText = "";
			if (modNPC is WormBody) {
				segmentText = "_Body";
			} else if (modNPC is WormTail) {
				segmentText = "_Tail";
			}
			yield return (PageName(modNPC) + segmentText, GetNPCStats(modNPC));
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
			int recipeGroup = item.GetGlobalItem<RecipeGroupTrackerGlobalItem>().recipeGroup;
			if (recipeGroup != -1) {
				text = $"<a is=a-link image=\"RecipeGroups/{RecipeGroupPage.GetRecipeGroupWikiName(recipeGroup)}\" href=Recipe_Groups>{RecipeGroup.recipeGroups[recipeGroup].GetText()}</a>";
			} else if (item.ModItem?.Mod is Origins) {
				text = $"<a is=a-link image=$fromStats>{name}{(string.IsNullOrWhiteSpace(note) ? "" : $"<note>{note}</note>")}</a>";
			} else if (WikiPageExporter.LinkFormatters.TryGetValue(item.ModItem?.Mod, out var formatter)) {
				text = $"{formatter(item.Name)}";
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
		public static (List<Recipe> recipes, List<Recipe> usedIn) GetRecipes(Item item) {
			List<Recipe> recipes = [];
			List<Recipe> usedIn = [];
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				if (recipe.Disabled) continue;
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
					recipes.Add(CreateFakeRecipe(item.type).AddIngredient(i).AddCondition(ShimmerTransmutationWikiCondition));
					break;
				}
			}
			if (ItemID.Sets.ShimmerTransformToItem[item.type] != -1) {
				usedIn.Add(CreateFakeRecipe(ItemID.Sets.ShimmerTransformToItem[item.type]).AddIngredient(item.type).AddCondition(ShimmerTransmutationWikiCondition));
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
	public enum WeaponTypes {
		None,
		Sword,
		Spear,
		Boomerang,
		Yoyo,
		Bow,
		Gun,
		HarpoonGun,
		Wand,
		MagicGun,
		SpellBook,
		Artifact,
		Minion,
		Sentry,
		Whip,
		Incantation,
		ThrownExplosive,
		Handcannon,
		RocketLauncher,
		CanisterLauncher,
		OtherMelee,
		OtherRanged,
		OtherMagic,
		OtherSummon,
		OtherExplosive
	}
}
