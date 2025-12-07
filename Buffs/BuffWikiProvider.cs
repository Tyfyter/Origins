using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Items.Weapons.Ammo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;

namespace Origins.Buffs {
	public class BuffWikiProvider : WikiProvider<ModBuff> {
		public override string PageName(ModBuff buff) {
			return WikiPageExporter.GetWikiName(buff);
		}
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(ModBuff buff) {
			return default;
			Dictionary<string, object> context = new() {
				["Name"] = WikiPageExporter.GetWikiName(buff),
				["DisplayName"] = Lang.GetItemNameValue(buff.Type)
			};
			IEnumerable<(string name, LocalizedText text)> pageTexts;
			if (buff is ICustomWikiStat wikiStats) {
				context["PageTextMain"] = wikiStats.PageTextMain.Value;
				pageTexts = wikiStats.PageTexts;
			} else {
				string key = $"WikiGenerator.{buff.Mod?.Name}.{buff.LocalizationCategory}.{context["Name"]}.MainText";
				context["PageTextMain"] = Language.GetOrRegister(key).Value;
				pageTexts = WikiPageExporter.GetDefaultPageTexts(buff);
			}
			foreach ((string name, LocalizedText text) text in pageTexts) {
				context[text.name] = text.text;
			}
			return (WikiPageExporter.WikiTemplate, context);
		}
		public override IEnumerable<(string, JObject)> GetStats(ModBuff buff) {
			JObject data = [];
			ICustomWikiStat customStat = buff as ICustomWikiStat;
			data["Image"] = buff.Texture.Replace(buff.Mod.Name, "§ModImage§");
			data["Name"] = Lang.GetBuffName(buff.Type);
			JArray types = new(Main.debuff[buff.Type] ? "Debuff" : "Buff");
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			data.Add("Types", types);
			data.AppendJStat("Tooltip", [..Lang.GetBuffDescription(buff.Type).Split('\n')], new JArray());
			if (customStat is not null) {
				string baseKey = $"WikiGenerator.Stats.{buff.Mod?.Name}.{buff.Name}.";
				foreach (string stat in customStat.LocalizedStats) {
					data.AppendStat(stat, Language.GetOrRegister(baseKey + stat).Value, "");
				}
				customStat?.ModifyWikiStats(data);
			}
			data.AppendStat("SpriteWidth", 32, 0);
			data.AppendStat("InternalName", buff.Name, null);
			yield return (customStat?.CustomStatPath ?? PageName(buff), data);
		}
	}
}
