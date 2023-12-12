using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Dev {
	public abstract class WikiSpecialPage : ILoadable, ICustomWikiStat, ILocalizedModType {
		public virtual string LocalizationCategory => "SpecialWikiPages";
		public Mod Mod { get; protected internal set; }
		/// <summary>
		/// The internal name of this.
		/// </summary>
		public virtual string Name => GetType().Name;
		public string FullName => Mod.Name + "/" + Name;
		public virtual LocalizedText DisplayName => Language.GetOrRegister(this.GetLocalizationKey("PageName").Replace("Mods.", "WikiGenerator."), () => Regex.Replace(Name, "(\\w)_?([A-Z])", "$1 $2"));
		public virtual LocalizedText Template => Language.GetOrRegister(this.GetLocalizationKey("Template").Replace("Mods.", "WikiGenerator."));
		public virtual string GeneratePage() {
			Dictionary<string, object> context = new() {
				["Name"] = Name,
				["DisplayName"] = DisplayName
			};
			ModifyContext(context);
			return BaseTemplate.Resolve(new() {
				["Name"] = Name,
				["DisplayName"] = DisplayName,
				["PageText"] = WikiTemplate.Resolve(context)
			});
		}
		public virtual void ModifyContext(Dictionary<string, object> context) { }
		#region template management
		PageTemplate wikiTemplate;
		string wikiTemplateLastText;
		public PageTemplate WikiTemplate {
			get {
				string templateText = Template.Value;
				if (wikiTemplate is null || templateText != wikiTemplateLastText) {
					wikiTemplate = new(templateText);
					wikiTemplateLastText = templateText;
				}
				return wikiTemplate;
			}
		}
		static PageTemplate baseTemplate;
		static DateTime baseTemplateWriteTime;
		public static PageTemplate BaseTemplate {
			get {
				if (baseTemplate is null || File.GetLastWriteTime(DebugConfig.Instance.WikiSpecialTemplatePath) > baseTemplateWriteTime) {
					baseTemplate = new(File.ReadAllText(DebugConfig.Instance.WikiSpecialTemplatePath));
					baseTemplateWriteTime = File.GetLastWriteTime(DebugConfig.Instance.WikiSpecialTemplatePath);
				}
				return baseTemplate;
			}
		}
		public void Load(Mod mod) {
			Mod = mod;
			//WikiPageExporter.InterfaceReplacesGenericClassProvider.Add(typeof(IWikiArmorSet));
			if (SpecialPages is null) SpecialPages = new();
			SpecialPages.Add(this);
		}
		public void Unload() {
			wikiTemplate = null;
			baseTemplate = null;
			SpecialPages.Remove(this);
			if (SpecialPages.Count <= 0) SpecialPages = null;
		}
		#endregion
		public static List<WikiSpecialPage> SpecialPages { get; private set; }
	}
}
