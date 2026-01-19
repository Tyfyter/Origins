using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Other.Fish;
using Origins.Items.Weapons.Melee;
using Origins.Layers;
using Origins.LootConditions;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Reflection;
using Origins.UI;
using Origins.UI.Event;
using PegasusLib;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.IO;

namespace Origins {
	public class OriginConfig : ModConfig {
		const string add_debuff_tooltip = "$Mods.Origins.Configs.OriginConfig.AddDebuff";
		public static OriginConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("VanillaBuffs")]

		[DefaultValue(true)]
		public bool WoodBuffs = true;
		[DefaultValue(true)]
		public bool RainSetBuff = true;
		[LabelKey($"$ItemName.{nameof(ItemID.ThunderStaff)}"), DefaultValue(true)]
		[TooltipKey(add_debuff_tooltip), TooltipArgs($"$ItemName.{nameof(ItemID.ThunderStaff)}", "$Buffs.Static_Shock_Debuff.DisplayName")]
		public bool ThunderStaff = true;
		[LabelKey($"$ItemName.{nameof(ItemID.ThunderSpear)}"), DefaultValue(true)]
		[TooltipKey(add_debuff_tooltip), TooltipArgs($"$ItemName.{nameof(ItemID.ThunderSpear)}", "$Buffs.Static_Shock_Debuff.DisplayName")]
		public bool ThunderSpear = true;
		[DefaultValue(true)]
		public bool VanillaWhipScale = true;

		[DefaultValue(true), ReloadRequired]
		public bool RoyalGel = true;

		[DefaultValue(true), ReloadRequired]
		public bool VolatileGelatin = true;

		[DefaultValue(true), ReloadRequired]
		public bool FrostHydra = true;

		[Header("Other")]

		[DefaultValue(true)]
		public bool Assimilation = true;

		[Header("Balance")]

		[JsonDefaultDictionaryKeyValue("{\"Mod\": \"Terraria\", \"Name\": \"GenericDamageClass\"}")]
		[JsonIgnore, ShowDespiteJsonIgnore]
		public Dictionary<DamageClassDefinition, float> StatShareRatio { get; set; } = new() {
			[new("Terraria/SummonDamageClass")] = 0.25f
		};

		[JsonIgnore]
		public static bool GraveshieldZombiesShouldDropAsItem => ServerSideAccessibility.Instance.GraveshieldZombiesDropAsItem && !Main.getGoodWorld;

		[ReloadRequired]
		[DefaultValue(true)]
		public bool GrassMerge = true;

		[DefaultValue(true)]
		public bool TicketInBank = true;
		internal void Save() {
			Directory.CreateDirectory(ConfigManager.ModConfigPath);
			string filename = Mod.Name + "_" + Name + ".json";
			string path = Path.Combine(ConfigManager.ModConfigPath, filename);
			string json = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
			WikiPageExporter.WriteFileNoUnneededRewrites(path, json);
		}
		static string BalanceSaveBath => Path.Combine(ConfigManager.ModConfigPath, "OriginsBalanceConfig" + ".nbt");
		public override void OnLoaded() {
			LoadFromFile();
		}
		internal void LoadFromFile() {
			if (File.Exists(BalanceSaveBath)) Load(TagIO.FromFile(BalanceSaveBath));
		}
		internal void SaveToFile() {
			TagCompound balanceData = [];
			Save(balanceData);
			TagIO.ToFile(balanceData, BalanceSaveBath);
		}
		internal void Save(TagCompound tag) {
			TagCompound statShareData = [];
			foreach (KeyValuePair<DamageClassDefinition, float> item in StatShareRatio) {
				statShareData[item.Key.ToString()] = item.Value;
			}
			tag[nameof(StatShareRatio)] = statShareData;
		}
		internal void Load(TagCompound tag) {
			StatShareRatio = [];
			foreach (KeyValuePair<string, object> item in tag.SafeGet<TagCompound>(nameof(StatShareRatio), [])) {
				StatShareRatio[DamageClassDefinition.FromString(item.Key)] = item.Value is float value ? value : 1;
			}
		}
		internal void CloneTo(OriginConfig clone) {
			TagCompound balanceData = [];
			Save(balanceData);
			clone.Load(balanceData);
		}
	}
	public class OriginClientConfig : ModConfig {
		public static OriginClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(false)]
		public bool SetBonusDoubleTap = false;

		[DefaultValue(true)]
		public bool AnimatedRavel = true;

		[DefaultValue(0.2f), Range(0f, 1f), Increment(0.05f)]
		public float DefiledShaderJitter = 0.2f;

		[DefaultValue(0.1f), Range(0f, 1f), Increment(0.05f)]
		public float DefiledShaderNoise = 0.1f;

		[DefaultValue(10), Range(0f, 30f), Increment(0.5f)]
		public float DefiledShaderSpeed = 10;

		[DefaultValue(1f), Range(0, 2), Increment(0.1f)]
		public float ScreenShakeMultiplier = 1f;

		[DefaultValue(true)]
		public bool ExtraGooeyRivenGores = true;

		[DefaultValue(false)]
		public bool TwentyFourHourTime = false;

		[DefaultValue(false)]
		public bool ShowRarityInHotbar = false;

		[DefaultValue(true)]
		public bool ImproveChlorophyteBulletsPerformance = true;

		[DefaultValue(true)]
		public bool DyeLightSources = true;
		[DefaultValue(10), Range(0, 60)]
		public int ProceduralLightSourceDyeRate = 10;

		[DefaultValue(ArtifactMinionHealthbarStyles.Auto)]
		public ArtifactMinionHealthbarStyles ArtifactMinionHealthbarStyle = ArtifactMinionHealthbarStyles.Auto;

		[DefaultValue(~QuestNotificationPositions.None), ConfigFlags<QuestNotificationPositions>, JsonConverter(typeof(FlagsEnumConverter<QuestNotificationPositions>))]
		public QuestNotificationPositions QuestNotificationPosition = ~QuestNotificationPositions.None;

		public LaserTagConfig laserTagConfig = new();

		[DefaultValue(true)]
		public bool OxfordComma = true;

		[Header("Journal")]
		[DefaultValue(true)]
		public bool ShowLockedEntries = true;
		[DefaultValue(true)]
		public bool EntryCategoryHeaders = true;
		[DefaultValue(Scroll_Wheel_Direction.Normal)]
		public Scroll_Wheel_Direction ScrollWheelDirection = Scroll_Wheel_Direction.Normal;

		[DefaultValue(Journal_Default_UI_Mode.Quest_List)]
		public Journal_Default_UI_Mode DefaultJournalMode = Journal_Default_UI_Mode.Quest_List;

		[Header("Compatibility"), ReloadRequired]
		public List<NPCDefinition> npcsNotToForceDialectOn = [];

		[DefaultValue(false)]
		public bool DisableCoolVisualEffects = false;
		internal void Save() {
			Directory.CreateDirectory(ConfigManager.ModConfigPath);
			string filename = Mod.Name + "_" + Name + ".json";
			string path = Path.Combine(ConfigManager.ModConfigPath, filename);
			string json = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
			WikiPageExporter.WriteFileNoUnneededRewrites(path, json);
		}
		[CustomModConfigItem(typeof(InconspicuousVersionElement))]
		public DebugConfig DebugMenuButton { get; set; }  = new();
		internal static bool forceReloadLanguage = false;
		public override void OnChanged() {
			if (forceReloadLanguage) {
				GameCulture culture = LanguageManager.Instance.ActiveCulture;
				GameCulture french = GameCulture.FromCultureName(GameCulture.CultureName.French);
				LanguageManager.Instance.SetLanguage(culture == french ? GameCulture.FromCultureName(GameCulture.CultureName.Italian) : french);
				LanguageManager.Instance.SetLanguage(culture);
			}
		}
	}
	public enum Scroll_Wheel_Direction {
		Normal,
		Inverted,
		Disabled
	}
	public class LaserTagConfig : ModConfig {
		public static LaserTagConfig Instance => OriginClientConfig.Instance.laserTagConfig;
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public override bool Autoload(ref string name) => false;
		[DefaultValue(Laser_Tag_Health_Pip_Placement.Back), DrawTicks]
		public Laser_Tag_Health_Pip_Placement HealthPipPlacement { get; set; } = Laser_Tag_Health_Pip_Placement.Back;
		[DefaultValue(6), Slider, Range(0, 32), DrawTicks, Increment(2)]
		public int HealthPipOffset { get; set; } = 6;
		[DefaultValue(false)]
		public bool HealthPipDirectionInverted { get; set; } = false;
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
			separatePagePanel = UIModConfig.MakeSeparateListPanel(Item, Value, MemberInfo, List, Index, Language.GetOrRegister("Mods.Origins.Configs.OriginClientConfig.DebugMenuButton.SecretLabel").ToString);
		}

		public override void Recalculate() {
			base.Recalculate();
			Height.Set(30, 0f);
		}
	}
	public class DebugConfig : ModConfig {
		public static DebugConfig Instance => OriginClientConfig.Instance.DebugMenuButton;
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public override bool Autoload(ref string name) => false;

		[DefaultValue(false)]
		public bool DebugMode = false;

		[DefaultValue(false)]
#if DEBUG
		[TooltipKey("$Mods.Origins.Configs.DebugConfig.ForceEnableDebugItems.DebugBuildTooltip")]
#endif
		public bool ForceEnableDebugItems = false;

		[DefaultValue(false)]
		public bool ForceAprilFools = false;
		#region exporting
		public string StatJSONPath { get; set; }
		public bool ExportAllItemStatsJSON {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(StatJSONPath)) {
						Origins.LogError($"StatJSONPath is null or whitespace");
						return;
					}
					if (Terraria.UI.ItemSlot.ShiftInUse) {
						Directory.CreateDirectory(StatJSONPath);
						int i;
						for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.ItemsByType[i].ModItem?.Mod is Origins) break;
						for (; i < ItemLoader.ItemCount; i++) {
							Item item = ContentSamples.ItemsByType[i];
							if (item.ModItem is not null) {
								if (item.ModItem?.Mod is not Origins) break;
								WikiPageExporter.ExportItemStats(item);
							}
						}
					} else {
						const string text = "Shift must be held to export all stats, for safety reasons";
						Origins.LogError(text);
						Main.NewText(text);
					}
				}
			}
		}
		public ItemDefinition ExportItemStatsJSON {
			get => default;
			set {
				if ((value?.Type ?? 0) > ItemID.None) {
					if (string.IsNullOrWhiteSpace(StatJSONPath)) {
						Origins.LogError($"StatJSONPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(StatJSONPath);
					WikiPageExporter.ExportItemStats(ContentSamples.ItemsByType[value.Type]);
				}
			}
		}
		public bool ExportAllNPCStatsJSON {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(StatJSONPath)) {
						Origins.LogError($"StatJSONPath is null or whitespace");
						return;
					}
					if (Terraria.UI.ItemSlot.ShiftInUse) {
						Directory.CreateDirectory(StatJSONPath);
						int i;
						for (i = 0; i < NPCLoader.NPCCount; i++) if (ContentSamples.NpcsByNetId[i].ModNPC?.Mod is Origins) break;
						for (; i < NPCLoader.NPCCount; i++) {
							NPC npc = ContentSamples.NpcsByNetId[i];
							if (npc.ModNPC is not null) {
								if (npc.ModNPC?.Mod is not Origins) break;
								WikiPageExporter.ExportNPCStats(npc);
							}
						}
					} else {
						const string text = "Shift must be held to export all stats, for safety reasons";
						Origins.LogError(text);
						Main.NewText(text);
					}
				}
			}
		}
		public bool ExportAllBuffStatsJSON {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(StatJSONPath)) {
						Origins.LogError($"StatJSONPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(StatJSONPath);
					for (int i = BuffID.Count; i < BuffLoader.BuffCount; i++) {
						ModBuff buff = BuffLoader.GetBuff(i);
						if (buff?.Mod is Origins) WikiPageExporter.ExportBuffStats(buff);
					}
				}
			}
		}
		public NPCDefinition ExportNPCStatsJSON {
			get => default;
			set {
				if ((value?.Type ?? 0) > NPCID.None) {
					if (string.IsNullOrWhiteSpace(StatJSONPath)) {
						Origins.LogError($"StatJSONPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(StatJSONPath);
					WikiPageExporter.ExportNPCStats(ContentSamples.NpcsByNetId[value.Type]);
				}
			}
		}
		public bool ExportAllNPCPages {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(WikiTemplatePath)) {
						Origins.LogError($"WikiTemplatePath is null or whitespace");
						return;
					}
					if (string.IsNullOrWhiteSpace(WikiPagePath)) {
						Origins.LogError($"WikiPagePath is null or whitespace");
						return;
					}
					if (Terraria.UI.ItemSlot.ShiftInUse) {
						Directory.CreateDirectory(WikiPagePath);
						int i;
						for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.NpcsByNetId[i].ModNPC?.Mod is Origins) break;
						for (; i < ItemLoader.ItemCount; i++) {
							NPC npc = ContentSamples.NpcsByNetId[i];
							if (npc.ModNPC is not null) {
								if (npc.ModNPC?.Mod is not Origins) break;
								if (npc.ModNPC is ICustomWikiStat { ShouldHavePage: false }) continue;
								if (npc.ModNPC is ICustomWikiStat { FullyGeneratable: true } || !File.Exists(WikiPageExporter.GetWikiPagePath(WikiPageExporter.GetWikiName(npc.ModNPC))))
									WikiPageExporter.ExportNPCPage(npc);
							}
						}
					} else {
						Main.NewText("Shift must be held to export all stats, for safety reasons");
					}
				}
			}
		}
		public NPCDefinition ExportNPCPage {
			get => default;
			set {
				if ((value?.Type ?? 0) != NPCID.None) {
					if (string.IsNullOrWhiteSpace(WikiTemplatePath)) {
						Origins.LogError($"WikiTemplatePath is null or whitespace");
						return;
					}
					if (string.IsNullOrWhiteSpace(WikiPagePath)) {
						Origins.LogError($"WikiPagePath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiPagePath);
					WikiPageExporter.ExportNPCPage(ContentSamples.NpcsByNetId[value.Type]);
				}
			}
		}
		public bool ExportAllItemPages {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(WikiTemplatePath)) {
						Origins.LogError($"WikiTemplatePath is null or whitespace");
						return;
					}
					if (string.IsNullOrWhiteSpace(WikiPagePath)) {
						Origins.LogError($"WikiPagePath is null or whitespace");
						return;
					}
					if (Terraria.UI.ItemSlot.ShiftInUse) {
						Directory.CreateDirectory(WikiPagePath);
						int i;
						for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.ItemsByType[i].ModItem?.Mod is Origins) break;
						for (; i < ItemLoader.ItemCount; i++) {
							Item item = ContentSamples.ItemsByType[i];
							if (item.ModItem is not null) {
								if (item.ModItem?.Mod is not Origins) break;
								if ((item.ModItem as ICustomWikiStat)?.ShouldHavePage == false) continue;
								WikiPageExporter.ExportItemPage(item);
							}
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
				if ((value?.Type ?? 0) > ItemID.None) {
					if (string.IsNullOrWhiteSpace(WikiTemplatePath)) {
						Origins.LogError($"WikiTemplatePath is null or whitespace");
						return;
					}
					if (string.IsNullOrWhiteSpace(WikiPagePath)) {
						Origins.LogError($"WikiPagePath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiPagePath);
					WikiPageExporter.ExportItemPage(ContentSamples.ItemsByType[value.Type]);
				}
			}
		}
		public bool ExportAllItemImages {
			get => default;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(WikiSpritesPath)) {
						Origins.LogError($"WikiSpritesPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiSpritesPath);
					int i;
					for (i = 0; i < ItemLoader.ItemCount; i++) if (ContentSamples.ItemsByType[i].ModItem?.Mod is Origins) break;
					for (; i < ItemLoader.ItemCount; i++) {
						Item item = ContentSamples.ItemsByType[i];
						if (item.ModItem is not null) {
							if (item.ModItem?.Mod is not Origins) break;
							if (item.ModItem is ICustomWikiStat { ShouldHavePage: false }) continue;
							WikiPageExporter.ExportContentSprites(item.ModItem);
						}
					}
				}
			}
		}
		public ItemDefinition ExportItemImages {
			get => default;
			set {
				if ((value?.Type ?? 0) > ItemID.None) {
					if (string.IsNullOrWhiteSpace(WikiSpritesPath)) {
						Origins.LogError($"WikiSpritesPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiSpritesPath);
					WikiPageExporter.ExportContentSprites(ContentSamples.ItemsByType[value.Type].ModItem);
				}
			}
		}
		public bool ExportAllNPCImages {
			get => default;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(WikiSpritesPath)) {
						Origins.LogError($"WikiSpritesPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiSpritesPath);
					int i;
					for (i = 0; i < NPCLoader.NPCCount; i++) if (ContentSamples.NpcsByNetId[i].ModNPC?.Mod is Origins) break;
					for (; i < NPCLoader.NPCCount; i++) {
						NPC npc = ContentSamples.NpcsByNetId[i];
						if (npc.ModNPC is not null) {
							if (npc.ModNPC?.Mod is not Origins) break;
							if (npc.ModNPC is ICustomWikiStat { ShouldHavePage: false }) continue;
							WikiPageExporter.ExportContentSprites(npc.ModNPC);
						}
					}
				}
			}
		}
		public NPCDefinition ExportNPCImages {
			get => default;
			set {
				if ((value?.Type ?? 0) > NPCID.None) {
					if (string.IsNullOrWhiteSpace(WikiSpritesPath)) {
						Origins.LogError($"WikiSpritesPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiSpritesPath);
					WikiPageExporter.ExportContentSprites(ContentSamples.NpcsByNetId[value.Type].ModNPC);
				}
			}
		}
		public bool ExportSpecialPages {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(WikiPagePath)) {
						Origins.LogError($"WikiPagePath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiPagePath);
					foreach (WikiSpecialPage item in WikiSpecialPage.SpecialPages) {
						if (item.GeneratePage() is string page) WikiPageExporter.WriteFileNoUnneededRewrites(WikiPageExporter.GetWikiPagePath(item.Name), page);
					}
				}
			}
		}
		public bool ExportSpecialImages {
			get => false;
			set {
				if (value) {
					if (string.IsNullOrWhiteSpace(WikiSpritesPath)) {
						Origins.LogError($"WikiSpritesPath is null or whitespace");
						return;
					}
					Directory.CreateDirectory(WikiSpritesPath);
					foreach (WikiSpecialPage item in WikiSpecialPage.SpecialPages) {
						foreach ((string name, Texture2D texture) in item.GetSprites() ?? Array.Empty<(string, Texture2D)>()) {
							WikiImageExporter.ExportImage(name, texture);
						}
						foreach ((string name, (Texture2D texture, int frames)[] textures) in item.GetAnimatedSprites() ?? Array.Empty<(string, (Texture2D texture, int frames)[])>()) {
							WikiImageExporter.ExportAnimatedImage(name, textures);
						}
					}
				}
			}
		}
		public string WikiTemplatePath { get; set; }
		public string WikiArmorTemplatePath { get; set; }
		public string WikiSpecialTemplatePath { get; set; }
		public string WikiSpritesPath { get; set; }
		public string WikiPagePath { get; set; }
		#endregion
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
					foreach (Accessory_Glow_Layer glowLayer in Origins.instance.GetContent<Accessory_Glow_Layer>()) {
						glowLayer.LoadAllTextures();
					}
					foreach (Accessory_Tangela_Layer tangelaLayer in Origins.instance.GetContent<Accessory_Tangela_Layer>()) {
						tangelaLayer.LoadAllTextures();
					}
					List<string> unused = [];
					HashSet<string> loadedAssets = AssetRepositoryMethods._assets.GetValue(Origins.instance.Assets).Keys.Select(k => k.Replace(Path.DirectorySeparatorChar, '/')).ToHashSet();
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
						if ((_asset.EndsWith('_') || _asset.EndsWith("__Glow")) && (_asset.StartsWith("Items/Armor/") || _asset.StartsWith("Items/Accessories/AccUseCatalogs"))) {
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
					WikiPageExporter.WriteFileNoUnneededRewrites(path, string.Join('\n', unused));
				}
			}
		}
		static void SearchLootForObtainability(List<DropRateInfo> dropInfoList, IEnumerable<IItemDropRule> rules) {
			DropRateInfoChainFeed ratesInfo = new(1f);
			foreach (IItemDropRule rule in rules) {
				rule.ReportDroprates(dropInfoList, ratesInfo);
				/*foreach (DropAsSetRule dropAsSetRule in rule.ChainedRules.Select(a => a.RuleToChain).FindDropRules<DropAsSetRule>()) {
					SearchLootForObtainability(dropInfoList, dropAsSetRule.ChainedRules.Select(a => a.RuleToChain));
				}*/
			}
		}
		public bool CheckItemObtainability {
			get => default;
			set {
				if (value) {
					List<string> unobtainable = [];
					HashSet<int> obtainableItems = [];
					List<(int, List<int>)> recipeResultItems = [];
					for (int i = 0; i < Main.recipe.Length; i++) {
						Recipe recipe = Main.recipe[i];
						List<int> requiredItems = recipe.requiredItem.Where(item => item.ModItem?.Mod is Origins).Select(item => item.type).ToList();
						if (requiredItems.Count <= 0) {
							obtainableItems.Add(recipe.createItem.type);
						} else {
							recipeResultItems.Add((recipe.createItem.type, requiredItems));
						}
					}
					List<DropRateInfo> dropInfoList = [];
					SearchLootForObtainability(dropInfoList, ItemDropDatabaseMethods._entriesByNpcNetId.GetValue(Main.ItemDropsDB).Values
						.Concat(ItemDropDatabaseMethods._entriesByItemId.GetValue(Main.ItemDropsDB).Values)
						.SelectMany(l => l)
						.Concat(ItemDropDatabaseMethods._globalEntries.GetValue(Main.ItemDropsDB))
					);
					for (int i = 0; i < dropInfoList.Count; i++) {
						obtainableItems.Add(dropInfoList[i].itemId);
					}
					foreach (KeyValuePair<(int, int), int> item in TileLoaderMethods.tileTypeAndTileStyleToItemType.GetValue()) {
						obtainableItems.Add(item.Value);
					}
					foreach (Item item in TileLoaderMethods.tiles.GetValue().SelectMany(l => l.GetItemDrops(0, 0))) {
						obtainableItems.Add(item.type);
					}

					foreach (KeyValuePair<int, int> item in TileLoaderMethods.wallTypeToItemType.GetValue()) {
						obtainableItems.Add(item.Value);
					}
					foreach (ModWall wall in TileLoaderMethods.walls.GetValue()) {
						int drop = -1;
						wall.Drop(0, 0, ref drop);
						if (drop != -1) {
							obtainableItems.Add(drop);
						}
					}

					foreach (int itemType in Origins.instance.GetContent().SelectMany(c => c is IItemObtainabilityProvider provider ? provider.ProvideItemObtainability() : [])) {
						obtainableItems.Add(itemType);
					}
					foreach (NPCShop.Entry entry in NPCShopDatabase.AllShops.SelectMany(s => s is NPCShop shop ? shop.Entries : [])) {
						obtainableItems.Add(entry.Item.type);
					}
					for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++) {
						if (i != -1 && (i < ItemID.Count || obtainableItems.Contains(i))) {
							obtainableItems.Add(ItemID.Sets.ShimmerTransformToItem[i]);
						}
					}
					int tries = 0;
					while (recipeResultItems.Count > 0 && ++tries < 1000) {
						for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++) {
							if (i != -1 && obtainableItems.Contains(i)) {
								obtainableItems.Add(ItemID.Sets.ShimmerTransformToItem[i]);
							}
						}
						for (int i = recipeResultItems.Count; i --> 0;) {
							(int result, List<int> ingredients) = recipeResultItems[i];
							if (obtainableItems.Contains(result)) {
								recipeResultItems.RemoveAt(i);
								continue;
							}
							for (int j = ingredients.Count; j --> 0;) {
								if (obtainableItems.Contains(ingredients[j])) {
									ingredients.RemoveAt(j);
								}
							}
							if (ingredients.Count <= 0) {
								obtainableItems.Add(result);
							}
							if (obtainableItems.Contains(result)) {
								recipeResultItems.RemoveAt(i);
							}
						}
					}
					Dictionary<int, HashSet<int>> missingIngredients = [];
					for (int i = recipeResultItems.Count; i-- > 0;) {
						(int result, List<int> ingredients) = recipeResultItems[i];
						if (!missingIngredients.TryGetValue(result, out HashSet<int> missing)) {
							missingIngredients.Add(result, ingredients.ToHashSet());
						} else {
							foreach (int item in ingredients) {
								missing.Add(item);
							}
						}
					}
					foreach (ILoadable content in Origins.instance.GetContent()) {
						if (content is ModItem item && !obtainableItems.Contains(item.Type)) {
							if (missingIngredients.TryGetValue(item.Type, out HashSet<int> missing)) {
								unobtainable.Add($"{item.Name}: [{string.Join(", ", missing.Select(Lang.GetItemName))}]");
							} else {
								unobtainable.Add(item.Name);
							}
						}
					}
					unobtainable.Sort(new AssetPathComparer());
					for (int i = 0; i < unobtainable.Count - 1; i++) {
						string[] a = unobtainable[i].Split('/');
						string[] b = unobtainable[i + 1].Split('/');
						int minLength = a.Length < b.Length ? a.Length : b.Length;
						for (int j = 0; j < minLength - 1; j++) {
							if (a[j] != b[j]) {
								unobtainable.Insert(i + 1, "");
								break;
							}
						}
					}
					Directory.CreateDirectory(ConfigManager.ModConfigPath);
					string filename = nameof(Origins) + "_Unobtainable_Items.txt";
					string path = Path.Combine(ConfigManager.ModConfigPath, filename);
					WikiPageExporter.WriteFileNoUnneededRewrites(path, string.Join('\n', unobtainable));
				}
			}
		}
		public bool ExportVersionedLists {
			get => default;
			set {
				if (!value) return;
				string basePath = Path.Combine(Program.SavePathShared, "ModSources", "Origins");
				if (!Directory.Exists(basePath)) return;
				string path = Path.Combine(basePath, "Info", Origins.instance.Version.ToString());
				WriteFile(Origins.instance.GetContent<ModItem>().Select(item => item.Name), File.CreateText(path + "_Items.txt"));
				WriteFile(Origins.instance.GetContent<ModNPC>().Select(item => item.Name), File.CreateText(path + "_NPCs.txt"));

				static void WriteFile(IEnumerable<string> lines, StreamWriter writer) {
					foreach (string line in lines.Order()) writer.Write(line + "\n");
					writer.Flush();
					writer.Close();
				}
				ModContent.GetInstance<ISHIntegration>()?.SetupVersionTags();
			}
		}
		public HashSet<string> IgnoredCompatibilitySuggestions { get; set; } = [];
		public override bool NeedsReload(ModConfig pendingConfig) {
#if !DEBUG
			if (pendingConfig is DebugConfig realPending && (realPending.ForceEnableDebugItems != ForceEnableDebugItems)) return true;
#endif
			return base.NeedsReload(pendingConfig);
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
	public class OriginAccessibilityConfig : ModConfig {
		public static OriginAccessibilityConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;
		[DefaultValue(false)]
		public bool DisableDefiledWastelandsShader { get; set; }
	}
	public class ServerSideAccessibility : ModConfig {
		public static ServerSideAccessibility Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(false)]
		public bool GraveshieldZombiesDropAsItem = false;

		[DefaultValue(1f), Range(0, 1)]
		public float RivenAsimilationMultiplier = 1f;
	}
}
