using Hjson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Origins {
	public class OriginsModIntegrations : ILoadable {
		private static OriginsModIntegrations instance;
		Mod wikiThis;
		public static Mod WikiThis { get => instance.wikiThis; set => instance.wikiThis = value; }
		static string WikiURL => "https://tyfyter.github.io/OriginsWiki";
		static HashSet<string> wikiSiteMap;
		public void Load(Mod mod) {
			instance = this;
			if (!Main.dedServ && ModLoader.TryGetMod("Wikithis", out wikiThis)) {
				//WikiThis.Call("AddModURL", Origins.instance, "tyfyter.github.io/OriginsWiki");
				WikiThis.Call(
					"DelegateWiki", 
					Origins.instance.Name, 
					(Func<object, object, bool>) WikiPageExists,
					(Func<object, object, bool>) OpenWikiPage
				);
				Origins.instance.Logger.Info("Added Wikithis integration");
				wikiSiteMap = new HashSet<string>();
				using (WebClient client = new WebClient()) {
					client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
						if (e.Error is not null) {
							mod.Logger.Error(e.Error);
							return;
						}
						XDocument doc = XDocument.Parse(e.Result);
						var desc = doc.Descendants();
						foreach (var item in doc.Descendants()) if (item.Name.LocalName == "loc") {
								if (Regex.Match(item.Value, "(?<=\\/)[^\\/]*(?=.html)").Value is string s && !string.IsNullOrEmpty(s)) {
									wikiSiteMap.Add(s);
								}
						}
					};
					client.DownloadStringAsync(new Uri(WikiURL + "/sitemap.xml"));
				}
			}
		}
		public static bool WikiPageExists(object obj, object id) {
			if (wikiSiteMap is not null) {
				return wikiSiteMap.Contains(GetWikiPageName(obj));
			}
			return true;
		}
		public static bool OpenWikiPage(object obj, object id) {
			if (!Main.hasFocus) {
				return false;
			}
			Utils.OpenToURL(WikiURL + "/searchPage?" + GetWikiPageName(obj));
			return false;
		}
		public static string GetWikiPageName(object obj) {
			if (obj is ICustomWikiDestination other) {
				return other.WikiPageName;
			} else {
				if (obj is Item item) {
					if (item.ModItem is ICustomWikiDestination wItem) {
						return wItem.WikiPageName;
					} else {
						return ContentSamples.ItemsByType[item.type].Name.Replace(' ', '_');
					}
				} else if (obj is NPC npc) {
					if (npc.ModNPC is ICustomWikiDestination wNPC) {
						return wNPC.WikiPageName;
					} else {
						return npc.TypeName.Replace(' ', '_');
					}
				}
			}
			return null;
		}

		public void Unload() {
			instance = null;
			wikiSiteMap = null;
		}
	}
	public interface ICustomWikiDestination {
		string WikiPageName { get; }
	}
}
