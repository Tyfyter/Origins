using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace Origins.Dev {
	public class WikiPageExporter : ILoadable {
		public delegate string WikiLinkFormatter(string name);
		public static DictionaryWithNull<Mod, WikiLinkFormatter> LinkFormatters { get; private set; } = new();
		public void Load(Mod mod) {
			LinkFormatters[Origins.instance] = (t) => {
				string formattedName = t.Replace(" ", "_");
				return $"{t} | Images/{formattedName}.png | {formattedName}";
			};
			LinkFormatters[null] = (t) => {
				string formattedName = t.Replace(" ", "_");
				return $"{t} | $default | https://terraria.wiki.gg/wiki/{formattedName}";
			};
		}
		static PageTemplate wikiTemplate;
		static DateTime wikiTemplateWriteTime;
		static PageTemplate WikiTemplate {
			get {
				if (wikiTemplate is null || File.GetLastWriteTime(DebugConfig.Instance.WikiTemplatePath) > wikiTemplateWriteTime) {
					wikiTemplate = new(File.ReadAllText(DebugConfig.Instance.WikiTemplatePath));
					wikiTemplateWriteTime = File.GetLastWriteTime(DebugConfig.Instance.WikiTemplatePath);
				}
				return wikiTemplate;
			}
		}
		public static void ExportItemPage(Item item) {
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
			Dictionary<string, object> context = new() {
				["Name"] = (item.ModItem?.Name ?? ItemID.Search.GetName(item.type)),
				["DisplayName"] = Lang.GetItemNameValue(item.type)
			};
			if (recipes.Count > 0 || usedIn.Count > 0) {
				context["Crafting"] = true;
				if (recipes.Count > 0) {
					context["Recipes"] = recipes;
				}
				if (usedIn.Count > 0) {
					context["UsedIn"] = usedIn;
				}
			}
			Directory.CreateDirectory(DebugConfig.Instance.WikiPagePath);

			string filename = context["Name"] + ".html";
			string path = Path.Combine(DebugConfig.Instance.WikiPagePath, filename);
			File.WriteAllText(path, WikiTemplate.Resolve(context));
		}
		public void Unload() {
			wikiTemplate = null;
			LinkFormatters = null;
		}
	}
	public class PageTemplate {
		ITemplateSnippet[] snippets;
		public PageTemplate(string text) {
			snippets = Parse(text);
		}
		public string Resolve(Dictionary<string, object> context) => CombineSnippets(snippets, context);
		public ITemplateSnippet[] Parse(string text) {
			StringBuilder currentText = new();
			int ifDepth = 0;
			List<ITemplateSnippet> snippets = new();
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
						while (!char.IsWhiteSpace(text[i])) currentText.Append(text[i++]);
						switch (currentText.ToString()) {
							case "if":
							ifDepth += 1;
							break;
							case "endif":
							throw new ArgumentException($"invalid format, endif with no if at character {i}: {text}");
						}
						currentText.Clear();

						i++;
						while (text[i] != '\n') currentText.Append(text[i++]);
						string condition = currentText.ToString();
						currentText.Clear();

						while (ifDepth > 0) {
							switch (text[i]) {
								case '#': {
									int parsePos = i + 1;
									StringBuilder directiveParser = new();
									while (!char.IsWhiteSpace(text[parsePos])) directiveParser.Append(text[parsePos++]);
									switch (directiveParser.ToString()) {
										case "if":
										ifDepth += 1;
										break;
										case "endif":
										ifDepth -= 1;
										break;
									}
									if (ifDepth != 0) goto default;
									i += "endif".Length;
									break;
								}

								default:
								currentText.Append(text[i++]);
								break;
							}
						}
						snippets.Add(new ConditionalSnippit(condition, Parse(currentText.ToString())));
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
		static string CombineSnippets(ITemplateSnippet[] snippets, Dictionary<string, object> context) => string.Join(null, snippets.Select(s => s.Resolve(context)));
		public interface ITemplateSnippet {
			string Resolve(Dictionary<string, object> context);
		}
		public class PlainTextSnippit : ITemplateSnippet {
			readonly string text;
			public PlainTextSnippit(string text) => this.text = text;
			public string Resolve(Dictionary<string, object> context) => text;
		}
		public class ConditionalSnippit : ITemplateSnippet {
			readonly string condition;
			readonly ITemplateSnippet[] snippets;
			public ConditionalSnippit(string condition, ITemplateSnippet[] snippets) {
				this.condition = condition;
				this.snippets = snippets;
			}
			public string Resolve(Dictionary<string, object> context) {
				if (context.ContainsKey(condition)) return CombineSnippets(snippets, context);
				return null;
			}
		}
		public class VariableSnippit : ITemplateSnippet {
			readonly string name;
			public VariableSnippit(string name) {
				this.name = name;
			}
			public string Resolve(Dictionary<string, object> context) {
				object value = context[name];
				if (value is List<Recipe> recipes) {
					string GetItemText(Item item) {
						string text = $"[link {item.Name}]";
						if (item.ModItem?.Mod is Origins) {
							text = $"[link {item.Name} | $fromStats]";
						} else if (WikiPageExporter.LinkFormatters.TryGetValue(item.ModItem?.Mod, out var formatter)) {
							text = $"[link {formatter(item.Name)}]";
						}
						if (item.stack != 1) text += $"({item.stack})";
						return text;
					}
					StringBuilder builder = new();
					builder.AppendLine("{recipes");
					foreach (var group in recipes.GroupBy((r) => new RecipeRequirements(r))) {
						builder.AppendLine("{");
						if (group.Key.requirements.Length > 0) {
							builder.AppendLine("stations:[");
							foreach (var requirement in group.Key.requirements) {
								if (WikiPageExporter.LinkFormatters.TryGetValue(requirement.mod, out var formatter)) {
									builder.AppendLine($"[link {formatter(requirement.name)}]");
								} else {
									Origins.instance.Logger.Warn($"No wiki link formatter for mod {requirement.mod}, skipping requirement for {requirement.name}");
								}
							}
							builder.AppendLine("]");
						}
						builder.AppendLine("items:[");
						foreach (Recipe recipe in group) {
							builder.AppendLine("{");
							builder.AppendLine("result:" + GetItemText(recipe.createItem) + ",");
							builder.AppendLine("ingredients:[");
							for (int i = 0; i < recipe.requiredItem.Count; i++) {
								if (i > 0) builder.Append(",");
								builder.Append(GetItemText(recipe.requiredItem[i]));
							}
							builder.AppendLine();
							builder.AppendLine("]");
							builder.AppendLine("}");
						}
						builder.AppendLine("]");
						builder.AppendLine("}");
					}
					builder.Append("recipes}");
					return builder.ToString();
				}
				return value.ToString();
			}
		}
	}
	public class RecipeRequirements {
		public readonly (Mod mod, string name)[] requirements;
		public RecipeRequirements(Recipe recipe) {
			requirements =
				recipe.requiredTile.Select(t => (TileLoader.GetTile(t)?.Mod, Lang.GetMapObjectName(MapHelper.TileToLookup(t, 0))))
				/*.Concat(
					recipe.Conditions.Select(c => c.Description.Value)
				)*/.ToArray();
		}
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
}
