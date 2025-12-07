using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Armor {
	public class ArmorSetWikiProvider : WikiProvider<IWikiArmorSet> {
		public override string PageName(IWikiArmorSet set) => set.ArmorSetName;
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(IWikiArmorSet value) {
			if (value.SharedPageSecondary) return default;
			Dictionary<string, object> context = new() {
				["Name"] = value.MergedArmorSetName,
				["DisplayName"] = value.ArmorSetDisplayName
			};
			HashSet<int> heads = [];
			HashSet<int> bodies = [];
			HashSet<int> legss = [];
			List<Recipe> recipes = [];
			List<Recipe> usedIn = [];
			List<string> setNames = [];
			List<string> pieceNames = [];
			foreach (IWikiArmorSet set in GetSetAndSubSets(value)) {
				if (set.HeadItemID != ItemID.None) heads.Add(set.HeadItemID);
				if (set.BodyItemID != ItemID.None) bodies.Add(set.BodyItemID);
				if (set.LegsItemID != ItemID.None) legss.Add(set.LegsItemID);
				setNames.Add(set.ArmorSetName);
			}
			foreach (int item in heads) {
				if (ContentSamples.ItemsByType[item].ModItem is null) continue;
				pieceNames.Add(WikiPageExporter.GetWikiName(ContentSamples.ItemsByType[item].ModItem));
				(List<Recipe> curRecipes, List<Recipe> curUsedIn) = WikiExtensions.GetRecipes(ContentSamples.ItemsByType[item]);
				recipes.AddRange(curRecipes);
				usedIn.AddRange(curUsedIn);
			}
			foreach (int item in bodies) {
				if (ContentSamples.ItemsByType[item].ModItem is null) continue;
				pieceNames.Add(WikiPageExporter.GetWikiName(ContentSamples.ItemsByType[item].ModItem));
				(List<Recipe> curRecipes, List<Recipe> curUsedIn) = WikiExtensions.GetRecipes(ContentSamples.ItemsByType[item]);
				recipes.AddRange(curRecipes);
				usedIn.AddRange(curUsedIn);
			}
			foreach (int item in legss) {
				if (ContentSamples.ItemsByType[item].ModItem is null) continue;
				pieceNames.Add(WikiPageExporter.GetWikiName(ContentSamples.ItemsByType[item].ModItem));
				(List<Recipe> curRecipes, List<Recipe> curUsedIn) = WikiExtensions.GetRecipes(ContentSamples.ItemsByType[item]);
				recipes.AddRange(curRecipes);
				usedIn.AddRange(curUsedIn);
			}
			if (recipes.Count > 0 || usedIn.Count > 0) {
				context["Crafting"] = true;
				if (recipes.Count > 0) {
					context["Recipes"] = recipes;
				}
				if (usedIn.Count > 0) {
					context["UsedIn"] = usedIn;
				}
			}
			context["SetNames"] = setNames;
			context["PieceNames"] = pieceNames;
			IEnumerable<(string name, LocalizedText text)> pageTexts;
			if (value is ICustomWikiStat wikiStats) {
				context["PageTextMain"] = wikiStats.PageTextMain.Value;
				pageTexts = wikiStats.PageTexts;
			} else {
				string baseKey = $"WikiGenerator.{value.MergedArmorSetName}.Armor.{context["Name"]}.";
				context["PageTextMain"] = Language.GetOrRegister(baseKey + "MainText").Value;
				pageTexts = WikiPageExporter.GetDefaultPageTexts(baseKey);
			}
			foreach (var text in pageTexts) {
				context[text.name] = text.text;
			}
			return (WikiTemplate, context);
		}
		public override IEnumerable<(string, JObject)> GetStats(IWikiArmorSet set) {
			if (set.SharedPageSecondary) yield break;
			List<JObject> statGroups = [];
			string[] baseTypes = ["Item", "Armor", "ArmorSet"];
			foreach (var item in ApplySets(GetSetAndSubSets(set))) {
				JObject data = [];
				JArray types = new(baseTypes);
				foreach (string type in set.SetCategories) types.Add(type);
				data.Add("Types", types);
				if (set is ModType modSet) data.Add("Name", modSet.Mod.GetLocalization($"ArmorSets.{item.set.ArmorSetName}.DisplayName", () => item.set.ArmorSetName.Replace('_', ' ')).Value);
				data.Add("WikiName", item.set.ArmorSetName);
				data.Add("DisplayName", item.set.ArmorSetDisplayName);
				JArray images = [$"ArmorSets/{item.set.ArmorSetName}"];
				if (item.set.HasFemaleVersion) images.Add($"ArmorSets/{item.set.ArmorSetName}_Female");
				data.Add("Images", images);
				data.Add("Defense", (int)item.dummyPlayer.statDefense);
				data.Add("SetBonus", item.dummyPlayer.setBonus);
				data.Add("IconItem", item.set.IconItem);
				data.Add("Rarity", WikiPageExporter.GetWikiItemRarity(item.dummyPlayer.armor[0]));
				data.Add("Sell", (item.dummyPlayer.armor[0].value + item.dummyPlayer.armor[1].value + item.dummyPlayer.armor[2].value) / 5);
				statGroups.Add(data);
			}
			if (set.CreateMergedSet && statGroups.Count > 1) {
				JObject data = [];
				JArray types = new(baseTypes);
				foreach (string type in set.SetCategories) types.Add(type);
				data.Add("Types", types);
				if (set is ModType modSet) data.Add("Name", modSet.Mod.GetLocalization($"ArmorSets.{set.ArmorSetName}.DisplayName", () => set.ArmorSetName.Replace('_', ' ')).Value);
				data.Add("WikiName", set.MergedArmorSetName);
				data.Add("DisplayName", set.ArmorSetDisplayName);
				data.Add("SetBonus", statGroups[0].GetValue("SetBonus"));

				JArray maleSets = [];
				JArray femaleSets = [];
				string defenseString = "";
				for (int i = 0; i < statGroups.Count; i++) {
					JObject statGroup = statGroups[i];
					JArray setImages = statGroup.Value<JArray>("Images");
					maleSets.Add(setImages[0]);
					if (setImages.Count > 1) femaleSets.Add(setImages[1]);
					defenseString += $"<a is=a-link image={WikiPageExporter.GetWikiItemImagePath(ItemLoader.GetItem(statGroup.Value<int>("IconItem")))}></a>{statGroup.GetValue("Defense")}";
				}
				JArray images = [maleSets];
				if (femaleSets.Count > 0) images.Add(femaleSets);
				data.Add("Images", images);
				data.Add("Defense", defenseString);

				statGroups.Insert(0, data);
			}
			for (int i = 0; i < statGroups.Count; i++) {
				statGroups[i].Remove("IconItem");
				yield return (statGroups[i].Value<string>("WikiName"), statGroups[i]);
			}
			yield break;
		}
		public override IEnumerable<(string, Texture2D)> GetSprites(IWikiArmorSet value) {
			foreach (IWikiArmorSet set in GetSetAndSubSets(value)) {
				ItemSlotSet slotSet = default(ItemSlotSet) with {
					headSlot = new Item(set.HeadItemID).headSlot,
					bodySlot = new Item(set.BodyItemID).bodySlot,
					legSlot = new Item(set.LegsItemID).legSlot
				};
				string baseName = Path.Combine("ArmorSets", set.ArmorSetName);
				Player mainPlayer = Main.gameMenu ? Main.player[Main.maxPlayers] : Main.LocalPlayer;
				bool male = mainPlayer.Male;
				if (Main.gameMenu) {
					mainPlayer.Male = true;
					mainPlayer.hair = 1;
				}
				mainPlayer.headFrame.Y = 0;
				mainPlayer.bodyFrame.Y = 0;
				mainPlayer.legFrame.Y = 0;
				yield return (
					baseName + ((male || !set.HasFemaleVersion) ? "" : "_Female"),
					SpriteGenerator.GenerateArmorSprite(mainPlayer, slotSet)
				);
				if (set.HasFemaleVersion) {
					Player dummyPlayer = Main.player[Main.maxPlayers];
					dummyPlayer.CopyVisuals(mainPlayer);
					dummyPlayer.Male ^= true;
					dummyPlayer.hair = dummyPlayer.Male ? 1 : 6;
					yield return (
						baseName + (male ? "_Female" : ""),
						SpriteGenerator.GenerateArmorSprite(dummyPlayer, slotSet)
					);
				}
			};
		}
		public static List<IWikiArmorSet> GetSetAndSubSets(IWikiArmorSet baseSet) {
			List<IWikiArmorSet> sets = new() { baseSet };
			foreach (int item in baseSet.SharedPageItems) {
				if (ItemLoader.GetItem(item) is IWikiArmorSet set) {
					sets.Add(set);
				}
			}
			return sets;
		}
		public static IEnumerable<(Player dummyPlayer, IWikiArmorSet set)> ApplySets(IEnumerable<IWikiArmorSet> sets) {
			return sets.Select(set => {
				int dummyIndex;
				Player dummyPlayer;
				for (dummyIndex = 0; dummyIndex < Main.maxPlayers; dummyIndex++) {
					dummyPlayer = Main.player[dummyIndex];
					if (!dummyPlayer.active) {
						goto foundIndex;
					}
				}
				throw new Exception("Every single real player slot is in use and tML doesn't like us using the fake one for this");
				foundIndex:
				dummyPlayer = new();
				if (dummyPlayer.ModPlayers.Length == 0) {
					PlayerMethods.SetupPlayer(dummyPlayer);
				}
				for (int i = 0; i < dummyPlayer.armor.Length; i++) {
					dummyPlayer.armor[i].TurnToAir();
				}
				dummyPlayer.ResetEffects();
				dummyPlayer.armor[0].SetDefaults(set.HeadItemID);
				dummyPlayer.armor[1].SetDefaults(set.BodyItemID);
				dummyPlayer.armor[2].SetDefaults(set.LegsItemID);
				dummyPlayer.UpdateEquips(dummyIndex);
				dummyPlayer.UpdateArmorSets(dummyIndex);
				return (dummyPlayer, set);
			});
		}
		#region template management
		static PageTemplate wikiTemplate;
		static DateTime wikiTemplateWriteTime;
		public static PageTemplate WikiTemplate {
			get {
				if (wikiTemplate is null || File.GetLastWriteTime(DebugConfig.Instance.WikiArmorTemplatePath) > wikiTemplateWriteTime) {
					wikiTemplate = new(File.ReadAllText(DebugConfig.Instance.WikiArmorTemplatePath));
					wikiTemplateWriteTime = File.GetLastWriteTime(DebugConfig.Instance.WikiArmorTemplatePath);
				}
				return wikiTemplate;
			}
		}
		public override void Load() {
			//WikiPageExporter.InterfaceReplacesGenericClassProvider.Add(typeof(IWikiArmorSet));
		}
		public override void Unload() {
			wikiTemplate = null;
		}
		#endregion
	}
	public interface IWikiArmorSet {
		string ArmorSetName { get; }
		string MergedArmorSetName => ArmorSetName;
		string ArmorSetDisplayName => Language.GetOrRegister($"WikiGenerator.Stats.{(this is ModType modType ? modType.Mod.Name : "missingno")}.{ArmorSetName}.DisplayName").Value;
		bool CreateMergedSet => true;
		bool HasFemaleVersion => true;
		int IconItem => HeadItemID;
		int HeadItemID { get; }
		int BodyItemID { get; }
		int LegsItemID { get; }
		string[] SetCategories => [];
		IEnumerable<int> SharedPageItems {
			get {
				yield break;
			}
		}
		/// <summary>
		/// should return false for all items which are returned in another item's <see cref="SharedPageItems"/>
		/// </summary>
		bool SharedPageSecondary => false;
	}
}
