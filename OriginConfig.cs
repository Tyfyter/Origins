using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Reflection;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ObjectData;

namespace Origins {
	[Label("Settings")]
	public class OriginConfig : ModConfig {
		public static OriginConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("VanillaBuffs")]

		[Label("Infected Wood Items")]
		[DefaultValue(true)]
		public bool WoodBuffs = true;

		[Header("Other")]

		[Label("Universal Grass Merge")]
		[ReloadRequired]
		[DefaultValue(true)]
		public bool GrassMerge = true;
		internal void Save() {
			Directory.CreateDirectory(ConfigManager.ModConfigPath);
			string filename = Mod.Name + "_" + Name + ".json";
			string path = Path.Combine(ConfigManager.ModConfigPath, filename);
			string json = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
			File.WriteAllText(path, json);
		}
	}
	[Label("Client Settings")]
	public class OriginClientConfig : ModConfig {
		public static OriginClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Use Double Tap For Set Bonus Abilities")]
		[DefaultValue(false)]
		public bool SetBonusDoubleTap = false;

		[Header("Journal")]

		[Label("Alternate Journal Layout")]
		[DefaultValue(false)]
		public bool TabbyJournal = false;

		[Label("Open Journal Entries on Unlock")]
		[DefaultValue(true)]
		public bool OpenJournalOnUnlock = true;

		[Label("Animated Ravel Transformation")]
		[DefaultValue(true)]
		public bool AnimatedRavel = true;

		[DefaultValue(true)]
		public bool ExtraGooeyRivenGores = true;

		[CustomModConfigItem(typeof(InconspicuousVersionElement))]
		public DebugConfig debugMenuButton = new();
	}

	internal class InconspicuousVersionElement : ConfigElement<ModConfig> {
		private UIPanel separatePagePanel;
		public override void OnBind() {
			base.OnBind();
			this.OnLeftClick += (evt, el) => {
				if (Terraria.UI.ItemSlot.ShiftInUse) {
					UIModConfig.SwitchToSubConfig(separatePagePanel);
				} else {
					Platform.Get<IClipboard>().Value = Origins.instance.Version.ToString();
					Main.NewText("Copied version to clipboard");
				}
			};

			TextDisplayFunction = () => $"{Label}: {Origins.instance.Version}";
			if (Value is null) {
				ModConfig data = Activator.CreateInstance(MemberInfo.Type, nonPublic: true) as ModConfig;
				JsonConvert.PopulateObject(JsonDefaultValueAttribute?.Json ?? "{}", data, ConfigManager.serializerSettings);
				Value = data;
			}
			SetupList();
			Recalculate();
		}

		private void SetupList() {
			separatePagePanel = UIModConfig.MakeSeparateListPanel(Item, Value, MemberInfo, List, Index, Language.GetOrRegister("Mods.Origins.Configs.OriginClientConfig.debugMenuButton.SecretLabel").ToString);
		}

		public override void Recalculate() {
			base.Recalculate();
			Height.Set(30, 0f);
		}
	}
	public class DebugConfig : ModConfig {
		public static DebugConfig Instance => OriginClientConfig.Instance.debugMenuButton;
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public override bool Autoload(ref string name) => false;

		[DefaultValue(false)]
		public bool DebugMode = false;

		public string StatJSONPath { get; set; }
		public bool ExportAllStatsJSON {
			get => false;
			set {
				if (value && !string.IsNullOrWhiteSpace(StatJSONPath)) {
					if (Terraria.UI.ItemSlot.ShiftInUse) {
						Directory.CreateDirectory(StatJSONPath);
						int i;
						for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.ItemsByType[i].ModItem?.Mod is Origins) break;
						for (; i < ItemLoader.ItemCount; i++) {
							Item item = ContentSamples.ItemsByType[i];
							if (item.ModItem?.Mod is not Origins) break;
							WikiPageExporter.ExportItemStats(item);
						}
					} else {
						Main.NewText("Shift must be held to export all stats, for safety reasons");
					}
				}
			}
		}
		public ItemDefinition ExportItemStatsJSON {
			get => default;
			set {
				if ((value?.Type ?? 0) > ItemID.None && !string.IsNullOrWhiteSpace(StatJSONPath)) {
					Directory.CreateDirectory(StatJSONPath);
					WikiPageExporter.ExportItemStats(ContentSamples.ItemsByType[value.Type]);
				}
			}
		}
		public bool ExportAllItemPages {
			get => false;
			set {
				if (value && !string.IsNullOrWhiteSpace(WikiPagePath)) {
					if (Terraria.UI.ItemSlot.ShiftInUse) {
						Directory.CreateDirectory(WikiPagePath);
						int i;
						for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.ItemsByType[i].ModItem?.Mod is Origins) break;
						for (; i < ItemLoader.ItemCount; i++) {
							Item item = ContentSamples.ItemsByType[i];
							if (item.ModItem?.Mod is not Origins) break;
							if ((item.ModItem as ICustomWikiStat)?.ShouldHavePage == false) continue;
							if (((item.ModItem as ICustomWikiStat)?.FullyGeneratable ?? false) || !File.Exists(WikiPageExporter.GetWikiPagePath(WikiPageExporter.GetWikiName(item.ModItem))))
								WikiPageExporter.ExportItemPage(item);
						}
					} else {
						Main.NewText("Shift must be held to export all stats, for safety reasons");
					}
				}
			}
		}
		public ItemDefinition ExportItemPage {
			get => default;
			set {
				if ((value?.Type ?? 0) > ItemID.None && !string.IsNullOrWhiteSpace(WikiTemplatePath) && !string.IsNullOrWhiteSpace(WikiPagePath)) {
					Directory.CreateDirectory(WikiPagePath);
					WikiPageExporter.ExportItemPage(ContentSamples.ItemsByType[value.Type]);
				}
			}
		}
		public bool ExportAllItemImages {
			get => default;
			set {
				if (value && !string.IsNullOrWhiteSpace(WikiSpritesPath)) {
					Directory.CreateDirectory(WikiSpritesPath);
					int i;
					for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.ItemsByType[i].ModItem?.Mod is Origins) break;
					for (; i < ItemLoader.ItemCount; i++) {
						Item item = ContentSamples.ItemsByType[i];
						if (item.ModItem?.Mod is not Origins) break;
						if ((item.ModItem as ICustomWikiStat)?.ShouldHavePage == false) continue;
						WikiPageExporter.ExportItemSprites(item);
					}
				}
			}
		}
		public ItemDefinition ExportItemImages {
			get => default;
			set {
				if ((value?.Type ?? 0) > ItemID.None && !string.IsNullOrWhiteSpace(WikiTemplatePath) && !string.IsNullOrWhiteSpace(WikiPagePath)) {
					Directory.CreateDirectory(WikiSpritesPath);
					WikiPageExporter.ExportItemSprites(ContentSamples.ItemsByType[value.Type]);
				}
			}
		}
		public string WikiTemplatePath { get; set; }
		public string WikiArmorTemplatePath { get; set; }
		public string WikiSpritesPath { get; set; }
		public string WikiPagePath { get; set; }
		public bool CheckTextureUsage {
			get => default;
			set {
				if (value) {
					foreach (ILoadable content in Origins.instance.GetContent()) {
						if (content is ModItem item) {
							Main.instance.LoadItem(item.Type);
						} else if (content is ModProjectile proj) {
							Main.instance.LoadProjectile(proj.Type);
						} else if (content is ModNPC npc) {
							Main.instance.LoadNPC(npc.Type);
						} else if (content is ModTile tile) {
							Main.instance.LoadTiles(tile.Type);
						} else if (content is ModWall wall) {
							Main.instance.LoadWall(wall.Type);
						}
						if (content is ILoadExtraTextures extras) {
							extras.LoadTextures();
						}
					}
					List<string> unused = new();
					var loadedAssets = AssetRepositoryMethods._assets.GetValue(Origins.instance.Assets).Keys.Select(k => k.Replace(Path.DirectorySeparatorChar, '/')).ToHashSet();
					loadedAssets.Add("icon");
					loadedAssets.Add("Buffs/BuffTemplate");
					loadedAssets.Add("Buffs/DebuffTemplate");
					loadedAssets.Add("Items/Armor/Armor_Conversion");
					loadedAssets.Add("Items/Armor/ArmorTemplate_v1");
					loadedAssets.Add("NPCs/BossBarTemplate");
					loadedAssets.Add("Tiles/BossDrops/Boss_Trophy_Empty");
					loadedAssets.Add("Tiles/BossDrops/Boss_Trophy_Item_Empty");
					loadedAssets.Add("Tiles/BossDrops/Relic_Examples");
					loadedAssets.Add("Tiles/interesting_tile");
					loadedAssets.Add("Tiles/Tile_Template");
					foreach (string asset in Origins.instance.RootContentSource.EnumerateAssets()) {
						string _asset = Path.ChangeExtension(asset, null);
						if ((_asset.EndsWith("_") || _asset.EndsWith("__Glow")) && (_asset.StartsWith("Items/Armor/") || _asset.StartsWith("Items/Accessories/AccUseCatalogs"))) {
							continue;
						}
						if (!loadedAssets.Contains(_asset)) {
							unused.Add(_asset);
						}
					}
					unused.Sort(new AssetPathComparer());
					for (int i = 0; i < unused.Count - 1; i++) {
						string[] a = unused[i].Split('/');
						string[] b = unused[i + 1].Split('/');
						int minLength = a.Length < b.Length ? a.Length : b.Length;
						for (int j = 0; j < minLength - 1; j++) {
							if (a[j] != b[j]) {
								unused.Insert(i + 1, "");
								break;
							}
						}
					}
					Directory.CreateDirectory(ConfigManager.ModConfigPath);
					string filename = nameof(Origins) + "_Unused_Assets.txt";
					string path = Path.Combine(ConfigManager.ModConfigPath, filename);
					File.WriteAllText(path, string.Join('\n', unused));
				}
			}
		}
	}
	public class AssetPathComparer : IComparer<string> {
		public int Compare(string x, string y) {
			string[] a = x.Split('/');
			string[] b = y.Split('/');
			Comparer<string> comparer = Comparer<string>.Default;
			if (a.Length == b.Length) return comparer.Compare(x, y);
			int maxLength = a.Length > b.Length ? a.Length : b.Length;
			for (int i = 0; i < maxLength; i++) {
				if (i + 1 == a.Length) return 1;
				if (i + 1 == b.Length) return -1;
				int comp = comparer.Compare(a[i], b[i]);
				if (comp != 0) return comp;
			}
			return comparer.Compare(x, y);
		}
	}
}
