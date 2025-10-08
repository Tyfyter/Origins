using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Journal;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Dev.WikiPageExporter;
using static Tyfyter.Utils.ChestLootCache;

namespace Origins.Dev {
	public class ItemWikiProvider : WikiProvider<ModItem> {
		public override string PageName(ModItem modItem) {
			if (modItem.Item.rare == CursedRarity.ID) {
				return WikiPageExporter.GetWikiName(modItem) + "_(Cursed)";
			}
			return WikiPageExporter.GetWikiName(modItem);
		}
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(ModItem modItem) {
			Item item = modItem.Item;
			Dictionary<string, object> context = new() {
				["Name"] = WikiPageExporter.GetWikiName(modItem),
				["DisplayName"] = WikiPageExporter.ProcessTags(Lang.GetItemNameValue(item.type))
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
			foreach ((string name, LocalizedText text) in pageTexts) {
				context[name] = text;
			}
			return (WikiTemplate, context);
		}
		static Dictionary<int, string> sourceCache = null;
		static Dictionary<int, JArray> dropsCache = null;
		static Dictionary<int, JArray> recipesCache = null;
		static Dictionary<int, JArray> usedInCache = null;
		static void GenerateSourceCache() {
			if (sourceCache is not null) return;
			Dictionary<int, StringBuilder> cache = [];
			Dictionary<int, JArray> dropCache = [];
			StringBuilder GetBuilder(int type) {
				if (!cache.TryGetValue(type, out StringBuilder builder)) cache[type] = builder = new();
				return builder;
			}
			JArray GetDropCache(int type) {
				if (!dropCache.TryGetValue(type, out JArray drop)) dropCache[type] = drop = [];
				return drop;
			}
			for (int i = 0; i < Main.recipe.Length; i++) {
				if (Main.recipe[i].createItem.ModItem?.Mod is Origins && Main.recipe[i]?.Mod is Origins) {
					StringBuilder sources = GetBuilder(Main.recipe[i].createItem.type);
					RecipeRequirements reqs = new(Main.recipe[i]);
					string reqsText = reqs.requirements.Length > 0 ? $" (@ {string.Join(", ", reqs.requirements.Select(r => r.ToStringCompact()))})" : "";
					if (sources.Length > 0) sources.Append("<div class=divider></div>");
					sources.Append($"{string.Join(" + ", Main.recipe[i].requiredItem.Select(i => (i.stack > 1 ? $"({i.stack})" : "") + " " + WikiExtensions.GetItemText(ContentSamples.ItemsByType[i.type], imageOnly: true)))}{reqsText}");
				}
			}
			foreach (AbstractNPCShop shop in NPCShopDatabase.AllShops) {
				if (ShopTypes.TryGetValue(shop.GetType(), out var func)) {
					foreach (AbstractNPCShop.Entry entry in func(shop)) {
						if (entry.Item.ModItem?.Mod is Origins) {
							StringBuilder sources = GetBuilder(entry.Item.type);
							if (sources.Length > 0) sources.Append("<div class=divider></div>");
							sources.Append($"{WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[shop.NpcType])} (<a-coins>{entry.Item.GetStoreValue()}</a-coins>{string.Join(", ", entry.Conditions.Select(c => c.Description.Value))})");
						}
					}
				}
			}
			for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++) {
				if (ItemLoader.GetItem(ItemID.Sets.ShimmerTransformToItem[i])?.Mod is Origins) {
					StringBuilder sources = GetBuilder(ItemID.Sets.ShimmerTransformToItem[i]);
					if (sources.Length > 0) sources.Append("<div class=divider></div>");
					sources.Append(WikiExtensions.GetItemText(ContentSamples.ItemsByType[i]));
					sources.Append(" (@ <a is=a-link href=https://terraria.wiki.gg/wiki/Shimmer>Shimmer</a>)");
				}
			}
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(i);
				if (rules?.Count > 0) {
					List<DropRateInfo> drops = [];
					for (int j = 0; j < rules.Count; j++) {
						DropRateInfoChainFeed ratesInfo = new(1f);
						try {
							rules[j].ReportDroprates(drops, ratesInfo);
						} catch (Exception) {}
					}
					for (int j = 0; j < drops.Count; j++) {
						if (ItemLoader.GetItem(drops[j].itemId)?.Mod is not Origins) continue;
						StringBuilder sources = GetBuilder(drops[j].itemId);
						if (sources.Length > 0) sources.Append("<div class=divider></div>");
						sources.Append(WikiExtensions.GetItemText(ContentSamples.ItemsByType[i]));
						string conditions = null;
						if ((drops[j].conditions?.Count ?? 0) > 0) {
							conditions = string.Join(", ", drops[j].conditions.Select(ConvertSourceCondition));
							sources.Append(" " + conditions);
						}
						JObject jobj = new() {
							["Name"] = WikiExtensions.GetItemText(ContentSamples.ItemsByType[i]),
							["Rate"] = drops[j].dropRate,
							["Max"] = drops[j].stackMax,
							["Min"] = drops[j].stackMin,
							["Difficulty"] = "Normal"
						};
						if (!string.IsNullOrEmpty(conditions)) jobj["Notes"] = conditions;
						GetDropCache(drops[j].itemId).Add(jobj);
						jobj = (JObject)jobj.DeepClone();
						jobj["Difficulty"] = "Expert";
						GetDropCache(drops[j].itemId).Add(jobj);
						jobj = (JObject)jobj.DeepClone();
						jobj["Difficulty"] = "Master";
						GetDropCache(drops[j].itemId).Add(jobj);
					}
				}
			}
			for (int i = 0; i < NPCLoader.NPCCount; i++) {
				List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForNPCID(i);
				if (rules?.Count > 0) {
					rules.GetAllDropRates(out List<DropRateInfo> classic, out List<DropRateInfo> expert, out List<DropRateInfo> master);
					classic.RemoveAll(static i => i.conditions?.Any(c => c is Conditions.IsExpert or Conditions.IsMasterMode) ?? false);
					expert.RemoveAll(static i => i.conditions?.Any(c => c is Conditions.NotExpert or Conditions.IsMasterMode) ?? false);
					master.RemoveAll(static i => i.conditions?.Any(c => c is Conditions.NotExpert or Conditions.NotMasterMode) ?? false);
					HashSet<int> itemTypes = [];
					for (int j = 0; j < classic.Count; j++) {
						if (ItemLoader.GetItem(classic[j].itemId)?.Mod is not Origins) continue;
						string conditions = null;
						if ((classic[j].conditions?.Count ?? 0) > 0) {
							conditions = string.Join(", ", classic[j].conditions.Select(ConvertSourceCondition).Where(c => !string.IsNullOrEmpty(c)));
						}
						if (itemTypes.Add(classic[j].itemId)) {
							StringBuilder sources = GetBuilder(classic[j].itemId);
							if (sources.Length > 0) sources.Append("<div class=divider></div>");
							sources.Append(WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[i]));
							if (!string.IsNullOrEmpty(conditions)) sources.Append(" " + conditions);
						}
						JObject jobj = new() {
							["Name"] = WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[i]),
							["Rate"] = classic[j].dropRate,
							["Max"] = classic[j].stackMax,
							["Min"] = classic[j].stackMin,
							["Difficulty"] = "Normal"
						};
						if (!string.IsNullOrEmpty(conditions)) jobj["Notes"] = conditions;
						GetDropCache(classic[j].itemId).Add(jobj);
					}
					for (int j = 0; j < expert.Count; j++) {
						if (ItemLoader.GetItem(expert[j].itemId)?.Mod is not Origins) continue;
						string dropConditions = null;
						string sourceConditions = null;
						if ((expert[j].conditions?.Count ?? 0) > 0) {
							dropConditions = string.Join(", ", expert[j].conditions.Where(c => c is not Conditions.IsExpert).Select(r => r.GetConditionDescription()));
							sourceConditions = string.Join(", ", expert[j].conditions.Select(ConvertSourceCondition).Where(c => !string.IsNullOrEmpty(c)));
						}
						if (itemTypes.Add(expert[j].itemId)) {
							StringBuilder sources = GetBuilder(expert[j].itemId);
							if (sources.Length > 0) sources.Append("<div class=divider></div>");
							sources.Append(WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[i]));
							if (!string.IsNullOrEmpty(sourceConditions)) sources.Append(" " + sourceConditions);
						}
						JObject jobj = new() {
							["Name"] = WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[i]),
							["Rate"] = expert[j].dropRate,
							["Max"] = expert[j].stackMax,
							["Min"] = expert[j].stackMin,
							["Difficulty"] = "Expert"
						};
						if (!string.IsNullOrEmpty(dropConditions)) jobj["Notes"] = dropConditions;
						GetDropCache(expert[j].itemId).Add(jobj);
					}
					for (int j = 0; j < master.Count; j++) {
						if (ItemLoader.GetItem(master[j].itemId)?.Mod is not Origins) continue;
						string dropConditions = null;
						string sourceConditions = null;
						if ((master[j].conditions?.Count ?? 0) > 0) {
							dropConditions = string.Join(", ", master[j].conditions.Where(c => c is not Conditions.IsMasterMode).Select(r => r.GetConditionDescription()));
							sourceConditions = string.Join(", ", master[j].conditions.Select(ConvertSourceCondition).Where(c => !string.IsNullOrEmpty(c)));
						}
						if (itemTypes.Add(master[j].itemId)) {
							StringBuilder sources = GetBuilder(master[j].itemId);
							if (sources.Length > 0) sources.Append("<div class=divider></div>");
							sources.Append(WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[i]));
							if (!string.IsNullOrEmpty(sourceConditions)) sources.Append(" " + sourceConditions);
						}
						JObject jobj = new() {
							["Name"] = WikiExtensions.GetNPCText(ContentSamples.NpcsByNetId[i]),
							["Rate"] = master[j].dropRate,
							["Max"] = master[j].stackMax,
							["Min"] = master[j].stackMin,
							["Difficulty"] = "Master"
						};
						if (!string.IsNullOrEmpty(dropConditions)) jobj["Notes"] = dropConditions;
						GetDropCache(master[j].itemId).Add(jobj);
					}
				}
			}
			static string ConvertSourceCondition(IItemDropRuleCondition condition) {
				string conditionName = condition.GetType().Name;
				string key = $"WikiGenerator.Generic.DropConditions.{conditionName}";
				return Language.Exists(key) ? Language.GetTextValue(key) : condition.GetConditionDescription();
			}
			(int chestId, float flags) currentKey = default;
			foreach (var action in ChestLoot.Actions) {
				if (action.action == LootQueueAction.CHANGE_QUEUE) {
					currentKey = (action.param, action.weight);
					continue;
				}
				if (action.action == LootQueueAction.ENQUEUE) {
					StringBuilder sources = GetBuilder(action.param);
					if (sources.Length > 0) sources.Append("<div class=divider></div>");
					string chest = ConvertChest(currentKey.chestId);
					string flags = ConvertFlags(currentKey.flags);
					sources.Append($"Any {chest}");
					if (!string.IsNullOrEmpty(flags)) sources.Append($" {flags}");
				}
			}

			static string ConvertChest(int chestId) {
				string key = $"WikiGenerator.Generic.Tiles.ChestId.{ChestID.Search.GetName(chestId)}";
				if (!Language.Exists(key) || !ChestID.Search.ContainsId(chestId)) return null;
				return Language.GetTextValue(key);
			}

			static string ConvertFlags(float flags) {
				int typeVar = (int)flags;
				string pyramid = Language.GetTextValue($"WikiGenerator.Generic.Link.Pyramid");
				string pyramidCondition = (typeVar & 0b0011) switch {
					0b0011 => $"in a {pyramid}",
					0b0001 => $"not in a {pyramid}",
					_ => ""
				};
				string cavern = Language.GetTextValue($"WikiGenerator.Generic.Link.Cavern");
				string heightCondition = (typeVar & 0b1100) switch {
					0b0100 => $"above the {cavern} layer",
					0b1100 => $"in the {cavern} layer",
					_ => ""
				};
				return string.Join(" and ",
					new string[] { pyramidCondition, heightCondition }
					.Where(s => !string.IsNullOrEmpty(s)));
			}

			sourceCache = new(cache.Select(kvp => new KeyValuePair<int, string>(kvp.Key, kvp.Value.ToString())));
			dropsCache = dropCache;
			GenerateRecipesCache();
		}
		static void GenerateRecipesCache() {
			static JArray CreateRecipeEntry(List<Recipe> recipes) {
				JArray allRecipesJson = [];
				foreach (var group in recipes.GroupBy((r) => new RecipeRequirements(r))) {
					JObject recipeJson = [];
					if (group.Key.requirements.Length > 0) {
						JArray stationsJson = [];
						foreach (RecipeRequirement requirement in group.Key.requirements) {
							if (string.IsNullOrEmpty(requirement.ToString())) continue;
							stationsJson.Add($"{requirement}");
						}
						recipeJson.Add("stations", stationsJson);
					}

					JArray itemsJson = [];
					foreach (Recipe recipe in group) {
						JObject resultsObject = [];
						resultsObject.Add("result", $"{WikiExtensions.GetItemText(recipe.createItem)}");

						JArray ingredientsJson = [];
						foreach (Item requiredItem in recipe.requiredItem) {
							ingredientsJson.Add($"{WikiExtensions.GetItemText(requiredItem)}");
						}
						resultsObject.Add("ingredients", ingredientsJson);

						itemsJson.Add(resultsObject);
					}
					recipeJson.Add("items", itemsJson);
					allRecipesJson.Add(recipeJson);
				}
				return allRecipesJson;
			}

			Dictionary<int, JArray> recipesDict = [];
			Dictionary<int, JArray> usedInDict = [];
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				Item item = ContentSamples.ItemsByType[i];
				if (item?.ModItem?.Mod is not Origins) continue;

				(List<Recipe> recipes, List<Recipe> usedIn) = WikiExtensions.GetRecipes(ContentSamples.ItemsByType[i]);
				JArray recipeData = CreateRecipeEntry(recipes);
				if (recipeData.Count > 0) recipesDict.Add(i, recipeData);

				JArray usedInData = CreateRecipeEntry(usedIn);
				if (usedInData.Count > 0) usedInDict.Add(i, usedInData);
			}

			recipesCache = recipesDict;
			usedInCache = usedInDict;
		}
		public override IEnumerable<(string, JObject)> GetStats(ModItem modItem) {
			Item item = modItem.Item;
			JObject data = [];
			ICustomWikiStat customStat = item.ModItem as ICustomWikiStat;
			data["Image"] = customStat?.CustomSpritePath ?? WikiPageExporter.GetWikiItemImagePath(modItem);
			data["Name"] = WikiPageExporter.ProcessTags(item.Name);
			JArray types = new("Item");
			if (modItem is IJournalEntrySource) types.Add("Lore");
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			if (item.pick != 0 || item.axe != 0 || item.hammer != 0 || item.fishingPole != 0 || item.bait != 0) types.Add("Tool");
			if (item.accessory) types.Add("Accessory");
			if (ItemID.Sets.SortingPriorityBossSpawns[item.type] != -1) types.Add("BossSummon");
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
						if (item.useAmmo == ModContent.ItemType<Resizable_Mine_Wood>()) {
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
							} else if (item.useAmmo == ModContent.ItemType<Resizable_Mine_Wood>()) {
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
				else if (item.ammo == ModContent.ItemType<Metal_Slug>()) types.Add("Slug");
				break;
			}
			if (customStat?.Hardmode ?? (!item.consumable && item.rare > ItemRarityID.Orange)) types.Add("Hardmode");

			if (ItemID.Sets.IsFood[item.type]) types.Add("Food");
			if (item.ammo != 0 && item.ammo != ItemID.CopperOre) types.Add("Ammo");
			if (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1) types.Add("Armor");
			if (item.createTile != -1) {
				types.Add("Tile");
				if (TileID.Sets.Torch[item.createTile]) types.Add("Torch");
				if (TileID.Sets.Ore[item.createTile]) types.Add("Ore");
			}
			if (item.expert) types.Add("Expert");
			if (item.master) types.Add("Master");
			data.Add("Types", types);

			data.AppendStat("PickPower", item.pick, 0);
			data.AppendStat("AxePower", item.axe * 5, 0);
			data.AppendStat("HammerPower", item.hammer, 0);
			data.AppendStat("FishPower", item.fishingPole, 0);
			data.AppendStat("BaitPower", item.bait, 0);
			data.AppendStat("ReachBonus", item.tileBoost, 0);

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
			if (item.createWall != -1) {
				types.Add("Wall");
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
					string line = WikiPageExporter.ProcessTags(item.ToolTip.GetLine(i));
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
				GenerateSourceCache();
				if (Language.Exists(key)) data.AppendStat("Source", Language.GetTextValue(key), key);
				else {
					if (sourceCache.TryGetValue(item.type, out string source)) data.AppendStat("Source", source, "");
				}
				if (dropsCache.TryGetValue(item.type, out JArray dropSources)) data.AppendJStat<JArray>("DropSources", dropSources, []);
				if (recipesCache.TryGetValue(item.type, out JArray recipes)) data.AppendJStat<JArray>("Recipes", recipes, []);
				if (usedInCache.TryGetValue(item.type, out JArray usedIn)) data.AppendJStat<JArray>("UsedIn", usedIn, []);

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
			yield return (customStat?.CustomStatPath ?? PageName(modItem), data);
			if (item.makeNPC > 0 && NPCLoader.GetNPC(item.makeNPC) is ModNPC modNPC) {
				yield return ((customStat?.CustomStatPath ?? PageName(modItem)) + "_NPC", NPCWikiProvider.GetNPCStats(modNPC));
			}
		}
		public override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(ModItem value) {
			if (Main.itemAnimations[value.Type] is DrawAnimation animation) {
				yield return (WikiPageExporter.GetWikiName(value), SpriteGenerator.GenerateAnimationSprite(TextureAssets.Item[value.Type].Value, animation));
			}
			yield break;
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
