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
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Origins.Tiles.Defiled;
using Origins.Items.Materials;
using MonoMod.RuntimeDetour.HookGen;
//using ThoriumMod.Items;
using System.Reflection;
//using ThoriumMod.Projectiles.Bard;
using Origins.NPCs.MiscE;

namespace Origins {
	public class OriginsModIntegrations : ILoadable {
		private static OriginsModIntegrations instance;
		Mod wikiThis;
		public static Mod WikiThis { get => instance.wikiThis; set => instance.wikiThis = value; }
		Mod epikV2;
		public static Mod EpikV2 { get => instance.epikV2; set => instance.epikV2 = value; }
		Asset<Texture2D> phaseIndicator;
		public static Asset<Texture2D> PhaseIndicator { get => instance.phaseIndicator; set => instance.phaseIndicator = value; }
		Mod herosMod;
		public static Mod HEROsMod { get => instance.herosMod; set => instance.herosMod = value; }
		Mod thorium;
		public static Mod Thorium { get => instance.thorium; set => instance.thorium = value; }
		static string WikiURL => "https://tyfyter.github.io/OriginsWiki";
		static HashSet<string> wikiSiteMap;
		public void Load(Mod mod) {
			instance = this;
			if (!Main.dedServ && ModLoader.TryGetMod("Wikithis", out wikiThis)) {
				//WikiThis.Call("AddModURL", Origins.instance, "tyfyter.github.io/OriginsWiki");
				WikiThis.Call(
					"DelegateWiki",
					Origins.instance.Name,
					(Func<object, object, bool>)WikiPageExists,
					(Func<object, object, bool>)OpenWikiPage
				);
				Origins.instance.Logger.Info("Added Wikithis integration");
				wikiSiteMap = new HashSet<string>();
				using WebClient client = new WebClient();
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
			if (ModLoader.TryGetMod("ThoriumMod", out instance.thorium)) {
				LoadThorium();
			}
		}
		public static void LateLoad() {
			if (ModLoader.TryGetMod("PhaseIndicator", out Mod phaseIndicatorMod) && phaseIndicatorMod.RequestAssetIfExists("PhaseIndicator", out Asset<Texture2D> phaseIndicatorTexture)) {
				instance.phaseIndicator = phaseIndicatorTexture;
			}
			if (ModLoader.TryGetMod("EpikV2", out instance.epikV2)) {
				EpikV2.Call("AddModEvilBiome", ModContent.GetInstance<Defiled_Wastelands>());
				EpikV2.Call("AddModEvilBiome", ModContent.GetInstance<Riven_Hive>());
				/*EpikV2.Call("AddBiomeKey",
					ModContent.ItemType<Defiled_Biome_Keybrand>(),
					ModContent.ItemType<Defiled_Key>(),
					ModContent.TileType<Defiled_Dungeon_Chest>(),
					36,
					ItemID.CorruptionKey
				);*///just here so it can eventually be used
			}
			if (ModLoader.TryGetMod("HEROsMod", out instance.herosMod)) {
				HEROsMod.Call(
					"AddItemCategory",
					"Explosive",
					"Weapons",
					(Predicate<Item>)((Item i) => i.CountsAsClass<Explosive>())
				);
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
		[JITWhenModsEnabled("ThoriumMod")]
		void LoadThorium() {///TODO: unfalse if thorium
#if false

			MonoModHooks.Add(
				typeof(BardItem).GetMethod("SetDefaults", BindingFlags.Public | BindingFlags.Instance),
				(Action<Action<BardItem>, BardItem>)([JITWhenModsEnabled("ThoriumMod")](orig, self) => {
					orig(self);
					if (self is IBardDamageClassOverride classOverride) {
						self.Item.DamageType = classOverride.DamageType;
					}
				})
			);
			MonoModHooks.Add(
				typeof(BardProjectile).GetMethod("SetDefaults", BindingFlags.Public | BindingFlags.Instance),
				(Action<Action<BardProjectile>, BardProjectile>)([JITWhenModsEnabled("ThoriumMod")](orig, self) => {
					orig(self);
					if (self is IBardDamageClassOverride classOverride) {
						self.Projectile.DamageType = classOverride.DamageType;
					}
				})
			);
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.TheInnocent>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.FrostWormHead>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.FrostWormBody>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.FrostWormTail>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.SnowEater>());

			CorruptGlobalNPC.AssimilationAmounts.Add(ModContent.NPCType<ThoriumMod.NPCs.TheInnocent>(), 0.02f);
			CorruptGlobalNPC.AssimilationAmounts.Add(ModContent.NPCType<ThoriumMod.NPCs.SnowEater>(), 0.03f);

			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.TheStarved>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.HorrificCharger>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.VileFloater>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.ChilledSpitter>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.Freezer>());
			CorruptGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.SoulCorrupter>());

			CorruptGlobalNPC.AssimilationAmounts.Add(ModContent.NPCType<ThoriumMod.NPCs.TheStarved>(), 0.10f);
			CorruptGlobalNPC.AssimilationAmounts.Add(ModContent.NPCType<ThoriumMod.NPCs.Freezer>(), 0.09f);

			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.LivingHemorrhage>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.Clot>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.Coolmera>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.FrozenFace>());

			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.BlisterPod>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.Blister>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.Coldling>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.FrozenGross>());
			CrimsonGlobalNPC.NPCTypes.Add(ModContent.NPCType<ThoriumMod.NPCs.EpiDermon>());

			CrimsonGlobalNPC.AssimilationAmounts.Add(ModContent.NPCType<ThoriumMod.NPCs.Blister>(), 0.01f);
#endif
		}
	}
	public interface ICustomWikiDestination {
		string WikiPageName { get; }
	}
	[ExtendsFromMod("ThoriumMod")]
	public class OriginsThoriumPlayer : ModPlayer {
		public bool altEmpowerment = false;
		public override void ResetEffects() {
			altEmpowerment = false;
		}
	}
	[ExtendsFromMod("ThoriumMod")]
	public class OriginsThoriumGlobalNPC : GlobalNPC {
		public override bool InstancePerEntity => true;
		int sonorousShredderHitCount = 0;
		int sonorousShredderHitTime = 0;
		public override void ResetEffects(NPC npc) {
			if(sonorousShredderHitTime > 0) {
				if (--sonorousShredderHitTime <= 0) {
					sonorousShredderHitCount = 0;
				}
			}
		}
		public bool SonorousShredderHit() {
			if (sonorousShredderHitCount < 4) {
				sonorousShredderHitCount++;
				sonorousShredderHitTime = 300;
				return false;
			} else {
				sonorousShredderHitCount = 0;
				sonorousShredderHitTime = 0;
				return true;
			}
		}
	}
	public interface IBardDamageClassOverride {
		DamageClass DamageType { get; }
	}
}
