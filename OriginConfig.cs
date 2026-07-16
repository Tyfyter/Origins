using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Origins.Core;
using Origins.CrossMod;
using Origins.CrossMod.Thorium.Items.Weapons.Bard;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Dyes;
using Origins.Items.Other.Fish;
using Origins.Items.Tools;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Layers;
using Origins.LootConditions;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Reflection;
using Origins.Tiles;
using Origins.UI;
using Origins.UI.Event;
using PegasusLib;
using ReLogic.OS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
		[DefaultValue(true)]
		public bool ForbiddenArmor = true;
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
		[DefaultValue(true)]
		public bool NewHarpoonsFromTheFuture = true;
		[DefaultValue(true)]
		public bool QuirkyEvilSpread = true;

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

		[DefaultValue(Adjusto_Hook.ControlSetting.Default)]
		public Adjusto_Hook.ControlSetting adjustoHookControlSetting = Adjusto_Hook.ControlSetting.Default;

		[DefaultValue(true)]
		public bool PreferSelectedHook = true;

		[DefaultValue(true)]
		public bool AnimatedRavel = true;

		[DefaultValue(0.2f), Range(0f, 1f), Increment(0.05f)]
		public float DefiledShaderJitter = 0.2f;

		[DefaultValue(0.1f), Range(0f, 1f), Increment(0.05f)]
		public float DefiledShaderNoise = 0.1f;

		[DefaultValue(10), Range(0f, 30f), Increment(0.5f)]
		public float DefiledShaderSpeed = 10;

		[DefaultValue(1f), Range(0, 4), Increment(0.1f)]
		public float ScreenShakeMultiplier = 1f;

		[DefaultValue(true)]
		public bool ExtraGooeyRivenGores = true;

		[DefaultValue(false)]
		public bool TwentyFourHourTime = false;

		[DefaultValue(false)]
		public bool ShowRarityInHotbar = false;

		[DefaultValue(true)]
		public bool ImproveChlorophyteBulletsPerformance = true;
		[DefaultValue(typeof(Color), "255, 255, 255, 255"), ColorNoAlpha]
		public Color FlashbangColor = Color.White;

		[DefaultValue(true)]
		public bool DyeLightSources = true;
		[DefaultValue(10), Range(0, 60)]
		public int ProceduralLightSourceDyeRate = 10;
		[DefaultValue(true)]
		public bool DyeLightTiles = true;

		[DefaultValue(ArtifactMinionHealthbarStyles.Auto)]
		public ArtifactMinionHealthbarStyles ArtifactMinionHealthbarStyle = ArtifactMinionHealthbarStyles.Auto;

		[DefaultValue(~QuestNotificationPositions.None), ConfigFlags<QuestNotificationPositions>, JsonConverter(typeof(FlagsEnumConverter<QuestNotificationPositions>))]
		public QuestNotificationPositions QuestNotificationPosition = ~QuestNotificationPositions.None;

		public LaserTagConfig laserTagConfig = new();

		[DefaultValue(true)]
		[LanguageSpecific(GameCulture.CultureName.English)]
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

		[DefaultValue(true)]
		public bool FixNonSolidTileBlurring = true;
		[Header("Performance")]

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
		public DebugConfig DebugMenuButton { get; set; } = new();
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
#else
		[ReloadRequired]
#endif
		public bool ForceEnableDebugItems = false;

		[DefaultValue(false)]
		public bool ForceAprilFools = false;
		#region wiki exporting
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
		static readonly Dictionary<Type, MethodInfo> loads = [];
		static void LoadAsset<T>(AutoLoadingAsset<T> asset) where T : class => asset.LoadAsset();
		[NoJIT]
		static void ExpectedUnusedAssets(StringBuilder builder) {
			string expectedUnusedAssets =
			"""
			*/ArmorTemplate_v1
			*WIP
			*WIPs
			*Example
			*Template
			*/Example*
			Items/Accessories/Timbre_of_Hell_HandsOff - Copy
			Items/Accessories/Timbre_of_Hell_HandsOff_Glow
			Items/Armor/Amber/Explosive_Resin*
			Items/Armor/Chambersite/Chambersite
			Items/Armor/Defiled/Defiled_Helmet_Head_EyesClosed
			Items/Armor/Eyndum/Armor_Temp3
			Items/Armor/Fiberglass/Fiberglass_Armor
			Items/Armor/Lost/Defiled_Exhaustion_Buff
			Items/Armor/Lost/Lost_Breastplate_Tangela
			Items/Armor/Lost/Lost_Helm_Tangela
			Buffs/Brine_Latcher_Debuff
			Buffs/Confection_Assimilation
			Buffs/Contagion_Assimilation
			Buffs/Hallow_Assimilation
			Buffs/Huff_Puff_Buff
			Buffs/Hunger_Debuff
			Buffs/Sugar_Crash_Debuff
			Buffs/Sugar_Rush_Debuff
			Buffs/Sugarcoat_Debuff
			Items/Other/Consumables/HolidayHairs/AntiCorruptionDay_Hair
			Items/Other/Consumables/HolidayHairs/MentalHealthAwarenessDay_Hair
			Items/Tools/Miter_Saw_Arm
			Items/Tools/Miter_Saw_Blade
			Items/Vanity/Dev/PlagueTexan/SceneYMK_Wings_Wings_AF
			Items/Weapons/Ammo/Canisters/Oil_Canister_II
			Items/Weapons/Demolitionist/Crystal_Grenade_*
			Items/Weapons/Demolitionist/Grenade_Lawnchair_Alt
			Items/Weapons/Demolitionist/Holy_Hand_Grenade_Alt
			Items/Weapons/Demolitionist/Meteor_P2*
			Items/Weapons/Demolitionist/Shrapnel_Dust*
			Items/Weapons/Demolitionist/Sonar_Dynamite_Sonar
			Items/Weapons/Felnum/*
			Items/Weapons/Magic/Nerve_Flan_P
			Items/Weapons/Melee/_Tyrfing
			Items/Weapons/Melee/Chromtain_Smasher
			Items/Weapons/Melee/Defiled_Biome_Blade
			Items/Weapons/Melee/Riven_Biome_Blade
			Items/Weapons/Melee/Rocket_Lance
			Items/Weapons/Melee/Soul_Snatcher_ELaunch
			Items/Weapons/Melee/Tyrfing_Shard
			Items/Weapons/Ranged/Firespit_Old
			Items/Weapons/Ranged/Tendon_Tear_Old
			Items/Weapons/Summoner/Minions/Cluesy_*
			Items/Weapons/Summoner/Minions/Stardust_Elemental
			Items/Weapons/Summoner/Minions/Terratotem_Orb
			Items/Weapons/Summoner/Minions/Terratotem_Tab_Mask_Side
			Items/Weapons/Summoner/Accretion_Ribbon_P
			Items/Weapons/Summoner/Forsaken_Desire*
			Items/Weapons/Summoner/Neutron_Soup_Flames*
			Items/Weapons/Generic_Weapon_P
			NPCs/Brine/Boss/Lost_Diver_
			NPCs/Brine/Boss/Lost_Diver_Dancing
			NPCs/Brine/Thing_of_nightmares_Ill_keep_in_the_backlog
			NPCs/Cubekon/*
			NPCs/Defiled/Boss/New/*
			NPCs/Dungeon/Electromancer
			NPCs/Dungeon/Illusionary
			NPCs/Dungeon/Illusionary_Copy
			NPCs/Dungeon/Illusionary_Glow
			NPCs/Defiled/Defiled_Brute_Old
			NPCs/Defiled/Defiled_Cyclops_Old
			NPCs/Defiled/Defiled_Cyclops_Old2
			NPCs/Defiled/Defiled_SRCWIP
			NPCs/Defiled/Defiled_Weaver
			NPCs/Defiled/Defiled_Weaver_Glow
			NPCs/Defiled/Defiled_Weaver_Tangela
			NPCs/Defiled/Tangela_Portrayal_Concept2
			NPCs/MiscB/Ichor_Storm
			NPCs/Nova Pillar/*
			NPCs/Repentance/*
			NPCs/Riven/World_Cracker/World_Cracker_Husk
			NPCs/Riven/World_Cracker/World_Cracker_Jelly
			NPCs/TownNPCs/The_Author
			Projectiles/Misc/Amoeba_Chain_End
			Projectiles/Misc/Smol_Bubbel
			Projectiles/Weapons/Black_Hole_Bomb_P
			Projectiles/Weapons/Bright_Sword_Cast_P
			Projectiles/Weapons/Lava_Cast_P_Alt
			Projectiles/Weapons/Seam_Beam_End
			Projectiles/Weapons/Seam_Beam_Mid
			Projectiles/Weapons/Seam_Beam_Start
			Projectiles/Weapons/Water_Cast_P
			Sounds/Custom/Ambience/AshenAmbience
			Sounds/Custom/Ambience/AshenSuspense
			Sounds/Custom/Ambience/DefiledAmbience
			Sounds/Custom/Ambience/RivenAmbience
			Sounds/Custom/Ambience/SCP3_Ambience
			Sounds/Seer/*
			Sounds/*/*_Sample
			Textures/Procedural/*
			Textures/All_Torn
			Textures/Cell_Noise
			Textures/Cell_Noise_Inverted
			Textures/DSTNoise
			Textures/Example
			Textures/Glow_Intensity
			Textures/Green_Channel
			Textures/Pale_Background
			Textures/Red_Channel
			Textures/SC_Mask
			Textures/Shimmer_Construct_Stars
			Textures/Strikethrough_Font
			Textures/Time
			Textures/Torn_Example
			Tiles/Ashen/Cargo_Elevator_Door_Backend
			Tiles/Ashen/Mechanical_Key_Node_State_Glow
			Tiles/Ashen/Purple_Mechanical_Switch*
			Tiles/Brine/FrameNumGuide
			Tiles/Cubekon/*
			Tiles/Decoration/Sheet_Metal_1
			Tiles/Decoration/Sheet_Metal_2
			Tiles/Decoration/Sheet_Metal_3
			Tiles/Decoration/Sheet_Metal_4
			Tiles/Dusk/Dusk_Stone_Item
			Tiles/MusicBoxes/Music_Box_DW_Old_Item
			Tiles/Other/Chambersite_Gemcorn
			Tiles/Other/Chambersite_Sapling
			Tiles/Other/Chambersite_Tree
			Tiles/Other/Chambersite_Tree_Branches
			Tiles/Other/Chambersite_Tree_Tops
			Tiles/Other/Eyndum_Bar_Tile
			Tiles/Other/Fiber_Back_Grid
			Tiles/Other/Fiberglass_Tile_Ish
			Tiles/Other/Fiberglass_Tile_New_Frames
			Tiles/Other/Fiberglass_Vines
			Tiles/Other/Formium_Bar_Tile
			Tiles/Other/Glass
			Tiles/Other/Nova_Accumulator_Glow
			Tiles/Other/Nova_Monolith_Item
			Tiles/Other/Pincushion_Tile
			Tiles/Other/Pincushion_Tile_Highlight
			UI/Lore/Journal_Scroll
			UI/Lore/Journal_Search
			Untitled1246_20260615214754
			""";//*/ github desktop doesn't recognize """ as string delimiters, so this will hopefully make it stop thinking the entire rest of the file is a comment
			builder.Append(Regex.Escape(expectedUnusedAssets));
			builder.Insert(0, '^');
			builder.Append('$');
			builder.Replace("\\n", "$|^");
			builder.Replace("\\*", ".*");
		}
		public static List<string> ListUnusedAssets(bool includeCrossMod = false) {
			StringBuilder builder = new();
			// check version so we don't forget that the list may be outdated
			if (Origins.instance.Version <= new Version(0, 5, 3, 21)) ExpectedUnusedAssets(builder);
			Regex expected = new(builder.ToString(), RegexOptions.Compiled);
			Assembly thisAssembly = Origins.instance.Code;
			MethodInfo doLoad = ((Delegate)LoadAsset<Texture2D>).Method.GetGenericMethodDefinition();
			foreach (ILoadable content in Origins.instance.GetContent()) {
				switch (content) {
					case ModItem item:
					Main.instance.LoadItem(item.Type);
					break;

					case ModProjectile proj:
					Main.instance.LoadProjectile(proj.Type);
					ModContent.RequestIfExists<Texture2D>(proj.GlowTexture, out _);
					break;

					case ModNPC npc:
					Main.instance.LoadNPC(npc.Type);
					if (npc is Glowing_Mod_NPC glowingNPC) _ = glowingNPC.GlowTexture;
					if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(npc.Type, out NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers) && drawModifiers.CustomTexturePath is string texPath) ModContent.RequestIfExists<Texture2D>(texPath, out _);
					break;

					case ModTile tile:
					Main.instance.LoadTiles(tile.Type);
					break;

					case ModWall wall:
					Main.instance.LoadWall(wall.Type);
					break;

					case ModTree tree:
					tree.GetTexture();
					tree.GetTopTextures();
					tree.GetBranchTextures();
					break;

					case ModCactus cactus:
					cactus.GetTexture();
					cactus.GetFruitTexture();
					break;
				}
				if (content is ILoadExtraTextures extras) {
					extras.LoadTextures();
				}
#if DEBUG
				foreach (FieldInfo @field in content.GetType().WalkWhile(t => t.Assembly == thisAssembly, t => t.BaseType).SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))) {
					TryLoadFieldAssets(@field, content);
				}
				foreach (PropertyInfo property in content.GetType().WalkWhile(t => t.Assembly == thisAssembly, t => t.BaseType).SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))) {
					if (property.PropertyType == typeof(Texture2D)) {
						property.GetValue(content);
					}
				}
#endif
			}
#if DEBUG
			foreach (Type type in AssemblyManager.GetLoadableTypes(Origins.instance.Code)) {
				if (type.Name.Contains('<')) continue;
				foreach (FieldInfo @field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
					TryLoadFieldAssets(@field, null);
				}
			}
			void TryLoadFieldAssets(FieldInfo @field, object instance) {
				if (@field.FieldType.IsGenericTypeDefinition) return;
				if (@field.DeclaringType.IsGenericTypeDefinition) return;
				object GetFieldValue() => @field.GetValue(instance);
				if (@field.FieldType.IsGeneric(typeof(AutoLoadingAsset<>))) {
					Type assetType = @field.FieldType.GenericTypeArguments[0];
					if (!loads.TryGetValue(assetType, out MethodInfo load)) loads[assetType] = load = doLoad.MakeGenericMethod(assetType);
					load.Invoke(null, [GetFieldValue()]);
				} else if (@field.FieldType.IsAssignableTo(typeof(IEnumerable<AutoLoadingTexture>))) {
					foreach (AutoLoadingTexture tex in (IEnumerable<AutoLoadingTexture>)GetFieldValue()) {
						tex.LoadAsset();
					}
				} else if (@field.FieldType.IsGeneric(typeof(Dictionary<,>)) && @field.FieldType.GenericTypeArguments[1].IsGeneric(typeof(AutoLoadingAsset<>))) {
					Type assetType = @field.FieldType.GenericTypeArguments[1].GenericTypeArguments[0];
					if (!loads.TryGetValue(assetType, out MethodInfo load)) loads[assetType] = load = doLoad.MakeGenericMethod(assetType);
					object dict = GetFieldValue();
					foreach (object tex in (IEnumerable)@field.FieldType.GetProperty(nameof(Dictionary<,>.Values)).GetValue(dict)) {
						load.Invoke(null, [tex]);
					}
				} else if (@field.FieldType == typeof(AutoGlowingTexture)) {
					((IBatchLoadable)GetFieldValue()).Load();
				}
			}
			AprilFoolsAssetSwitcher<AutoLoadingTexture>.ForAllAFAssets(a => a.LoadAsset());
#endif

			foreach (Accessory_Glow_Layer glowLayer in Origins.instance.GetContent<Accessory_Glow_Layer>()) {
				glowLayer.LoadAllTextures();
			}
			foreach (Accessory_Tangela_Layer tangelaLayer in Origins.instance.GetContent<Accessory_Tangela_Layer>()) {
				tangelaLayer.LoadAllTextures();
			}
			List<string> unused = [];
			HashSet<string> loadedAssets = AssetRepositoryMethods._assets.GetValue(Origins.instance.Assets).Keys.Select(k => k.Replace(Path.DirectorySeparatorChar, '/')).ToHashSet();
			loadedAssets.Add("icon");
			loadedAssets.Add("Hair/HairSource/ExampleHair");
			loadedAssets.Add("Items/Armor/Armor_Conversion");
			loadedAssets.Add("Items/Armor/index");
			loadedAssets.Add("NPCs/Brine/Food_Chain");
			loadedAssets.Add("Tiles/BossDrops/Boss_Trophy_Empty");
			loadedAssets.Add("Tiles/BossDrops/Boss_Trophy_Item_Empty");
			loadedAssets.Add("Tiles/BossDrops/Relic_Examples");
			loadedAssets.Add("Tiles/interesting_tile");
			loadedAssets.Add("Tiles/BossDrops/Boss_Trophy_Empty_Item");

			#region cross mod
			// Has to be done manually to some degree
			loadedAssets.Add("CrossMod/Fargos/Items/Aether_Orb");
			loadedAssets.Add("CrossMod/Fargos/Items/Ashen_Chest");
			loadedAssets.Add("CrossMod/Fargos/Items/AshenRenewal");
			loadedAssets.Add("CrossMod/Fargos/Items/AshenSupremeRenewal");
			loadedAssets.Add("CrossMod/Fargos/Items/Defiled_Chest");
			loadedAssets.Add("CrossMod/Fargos/Items/DefiledRenewal");
			loadedAssets.Add("CrossMod/Fargos/Items/DefiledSupremeRenewal");
			loadedAssets.Add("CrossMod/Fargos/Items/High_Powered_Green_Laser");
			loadedAssets.Add("CrossMod/Fargos/Items/Riven_Chest");
			loadedAssets.Add("CrossMod/Fargos/Items/RivenRenewal");
			loadedAssets.Add("CrossMod/Fargos/Items/RivenSupremeRenewal");

			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Defiled_Storage_Core");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Defiled_Storage_Unit");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Defiled_Storage_Unit_Glow");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Defiled_Storage_Unit_Item");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Defiled_Storage_Upgrade");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Encrusted_Storage_Core");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Encrusted_Storage_Unit");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Encrusted_Storage_Unit_Glow");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Encrusted_Storage_Unit_Item");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Encrusted_Storage_Upgrade");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Sanguinite_Storage_Core");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Sanguinite_Storage_Unit");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Sanguinite_Storage_Unit_Glow");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Sanguinite_Storage_Unit_Item");
			loadedAssets.Add("CrossMod/MagicStorage/Tiles/Sanguinite_Storage_Upgrade");
			#endregion
			Regex jimageRegex = new("(?<!\\\\)\\[(?<tag>jimage)(\\/(?<options>[^:]+))?:(?<text>.+?)(?<!\\\\)\\]", RegexOptions.Compiled);
			foreach (LanguageTree branch in TextUtils.LanguageTree.Find("Mods.Origins.Journal").GetDescendants()) {
				foreach (Match match in jimageRegex.Matches(branch.TextValue)) {
					string text = match.Groups["text"].Value;
					if (text.StartsWith("Origins/")) loadedAssets.Add(text["Origins/".Length..]);
				}
			}
			foreach (ModSceneEffect biome in Origins.instance.GetContent<ModSceneEffect>()) {
				if (biome.MapBackground is not null) loadedAssets.Add(biome.MapBackground["Origins/".Length..]);
				if (biome is ModBiome { BackgroundPath: string backgroundPath }) loadedAssets.Add(backgroundPath["Origins/".Length..]);
				if (biome is ModBiome { BestiaryIcon: string bestiaryIcon }) loadedAssets.Add(bestiaryIcon["Origins/".Length..]);
			}
			Span<string> altLibWorldVariants = [
				"Normal",
						"ForTheWorthy",
						"NotTheBees",
						"Anniversary",
						"DontStarve",
						"RemixWorld"
			];
			foreach (AltBiome biome in Origins.instance.GetContent<AltBiome>()) {
				if (biome.IconSmall is not null) loadedAssets.Add(biome.IconSmall["Origins/".Length..]);
				if (biome.WorldIcon is not null) {
					string @base = biome.WorldIcon["Origins/".Length..];
					for (int i = 0; i < altLibWorldVariants.Length; i++) {
						loadedAssets.Add(@base + altLibWorldVariants[i]);
					}
				}
			}
			LoadSounds(typeof(Origins.Sounds));
			foreach (Type type in typeof(Origins.Sounds).GetNestedTypes()) LoadSounds(type);
			foreach (AEnvironmentSound sound in EnvironmentSounds.AllSounds) {
				LoadSounds(sound.GetType(), sound);
			}
			LoadSounds(typeof(Keytar));
			void LoadSounds(Type type, object instance = null) {
				foreach (FieldInfo @field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
					if (@field.FieldType != typeof(SoundStyle)) continue;
					SoundStyle sound = (SoundStyle)@field.GetValue(@field.IsStatic ? null : instance);
					string soundPath = sound.SoundPath["Origins/".Length..];
					if (sound.Variants.Length > 0) {
						for (int i = 0; i < sound.Variants.Length; i++) {
							loadedAssets.Add(soundPath + sound.Variants[i]);
						}
					} else {
						loadedAssets.Add(soundPath);
					}
				}
			}
			foreach (string asset in Origins.instance.RootContentSource.EnumerateAssets()) {
				string _asset = Path.ChangeExtension(asset, null);
				if ((_asset.EndsWith('_') || _asset.EndsWith("__Glow")) && (_asset.StartsWith("Items/Armor/") || _asset.StartsWith("Items/Vanity/") || _asset.StartsWith("Items/Accessories/AccUseCatalogs"))) {
					continue;
				}
				if (_asset.StartsWith("Icons/")) continue;
				if (_asset.StartsWith("NPCs/Boss_Controller_")) continue;
				if (_asset.StartsWith("Hair/HairSource/")) continue;
				if (_asset.Contains("Unused/")) continue;
				if (!includeCrossMod && _asset.StartsWith("CrossMod/")) continue;
				if (expected.IsMatch(_asset)) continue;
				if (!loadedAssets.Contains(_asset)) {
					unused.Add(_asset);
				}
			}
			unused.Sort(new AssetPathComparer());
			return unused;
		}
		public bool CheckTextureUsage {
			get => default;
			set {
				if (value) {
					List<string> unused = ListUnusedAssets(!Terraria.UI.ItemSlot.ShiftInUse);
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
					unused.Insert(0, $"Total Unused Assets: {unused.Count(s => !string.IsNullOrWhiteSpace(s))}");
					string directory = Path.Combine(Program.SavePathShared, "ModSources", nameof(Origins));
					Directory.CreateDirectory(directory);
					string path = Path.Combine(directory, "Unused_Assets.txt");
					WikiPageExporter.WriteFileNoUnneededRewrites(path, string.Join('\n', unused));
				}
			}
		}
		internal static void SearchLootForObtainability([Out] List<DropRateInfo> dropInfoList, IEnumerable<IItemDropRule> rules) {
			DropRateInfoChainFeed ratesInfo = new(1f);
			foreach (IItemDropRule rule in rules) {
				rule.ReportDroprates(dropInfoList, ratesInfo);
				/*foreach (DropAsSetRule dropAsSetRule in rule.ChainedRules.Select(a => a.RuleToChain).FindDropRules<DropAsSetRule>()) {
					SearchLootForObtainability(dropInfoList, dropAsSetRule.ChainedRules.Select(a => a.RuleToChain));
				}*/
			}
		}
		readonly struct TileDropsIterator<T>(IEnumerator<T> enumerator) : IEnumerable<T>, IEnumerator<T> {
			public TileDropsIterator(IEnumerable<T> enumerable) : this(enumerable.GetEnumerator()) { }
			readonly T IEnumerator<T>.Current => enumerator.Current;
			readonly object IEnumerator.Current => enumerator.Current;
			readonly bool IEnumerator.MoveNext() {
				try {
					return enumerator.MoveNext();
				} catch (Exception) {
					return false;
				}
			}
			readonly void IEnumerator.Reset() => enumerator.Reset();
			readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
			readonly IEnumerator IEnumerable.GetEnumerator() => this;
			readonly void IDisposable.Dispose() { }
		}
		public static List<string> GetUnobtainableItems(bool includeExpected = false) {
			static bool ShouldBeUnobtainable(ModItem item) => ItemID.Sets.IsAPickup[item.Type] || ItemID.Sets.Deprecated[item.Type] || item is IExpectToBeUnobtainable || item is TileItem { IsDebug: true };
			HashSet<int> obtainableItems = [];
			void AddObtainableItem(int type) {
				obtainableItems.Add(type);
			}
			List<(int, List<int>)> recipeResultItems = [];
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				List<int> requiredItems = recipe.requiredItem.Where(item => item.ModItem?.Mod is Origins).Select(item => item.type).ToList();
				if (requiredItems.Count <= 0) {
					AddObtainableItem(recipe.createItem.type);
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
				AddObtainableItem(dropInfoList[i].itemId);
			}
			foreach (var item in TileLoaderMethods.tileTypeAndTileStyleToItemType.GetValue()) {
				AddObtainableItem(item.Value);
			}
			foreach (var item in TileLoaderMethods.tiles.GetValue().SelectMany(l => new TileDropsIterator<Item>(l.GetItemDrops(0, 0)))) {
				AddObtainableItem(item.type);
			}
			Dye_Item.dyeItems.ForEach(dye => AddObtainableItem(dye.Type));
			foreach (NPC npc in ContentSamples.NpcsByNetId.Values) AddObtainableItem(npc.catchItem);

			foreach (var item in TileLoaderMethods.wallTypeToItemType.GetValue()) {
				AddObtainableItem(item.Value);
			}
			foreach (var wall in TileLoaderMethods.walls.GetValue()) {
				int drop = -1;
				wall.Drop(0, 0, ref drop);
				if (drop != -1) {
					AddObtainableItem(drop);
				}
			}

			foreach (int itemType in Origins.instance.GetContent().SelectMany(c => c is IItemObtainabilityProvider provider ? provider.ProvideItemObtainability() : [])) {
				AddObtainableItem(itemType);
			}
			foreach (NPCShop.Entry entry in NPCShopDatabase.AllShops.SelectMany(s => s is NPCShop shop ? shop.Entries : [])) {
				AddObtainableItem(entry.Item.type);
			}
			for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++) {
				if (i != -1 && (i < ItemID.Count || obtainableItems.Contains(i))) {
					AddObtainableItem(ItemID.Sets.ShimmerTransformToItem[i]);
				}
			}
			int tries = 0;
			while (recipeResultItems.Count > 0 && ++tries < 1000) {
				for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++) {
					if (i != -1 && obtainableItems.Contains(i)) {
						AddObtainableItem(ItemID.Sets.ShimmerTransformToItem[i]);
					}
				}
				for (int i = recipeResultItems.Count; i-- > 0;) {
					(int result, List<int> ingredients) = recipeResultItems[i];
					if (obtainableItems.Contains(result)) {
						recipeResultItems.RemoveAt(i);
						continue;
					}
					for (int j = ingredients.Count; j-- > 0;) {
						if (obtainableItems.Contains(ingredients[j])) {
							ingredients.RemoveAt(j);
						}
					}
					if (ingredients.Count <= 0) {
						AddObtainableItem(result);
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
			List<string> unobtainable = [];
			foreach (ModItem item in Origins.instance.GetContent<ModItem>()) {
				if (!includeExpected && ShouldBeUnobtainable(item)) continue;
				if (!obtainableItems.Contains(item.Type)) {
					if (missingIngredients.TryGetValue(item.Type, out HashSet<int> missing)) {
						unobtainable.Add($"{item.Name}: [{string.Join(", ", missing.Select(Lang.GetItemName))}]");
					} else {
						unobtainable.Add(item.Name);
					}
				}
			}
			unobtainable.Sort(new AssetPathComparer());
			List<string> overobtainable = [];
			if (!includeExpected) {
				foreach (ModItem item in Origins.instance.GetContent<ModItem>()) {
					if (!ShouldBeUnobtainable(item)) continue;
					if (obtainableItems.Contains(item.Type)) {
						if (missingIngredients.TryGetValue(item.Type, out HashSet<int> missing)) {
							overobtainable.Add($"{item.Name}: [{string.Join(", ", missing.Select(Lang.GetItemName))}]");
						} else {
							overobtainable.Add(item.Name);
						}
					}
				}
				overobtainable.Sort(new AssetPathComparer());
				if (overobtainable.Count > 0) {
					overobtainable.Insert(0, "");
					overobtainable.Insert(1, "Obtainable, but marked as unobtainable:");
				}
				unobtainable.AddRange(overobtainable);
			}
			return unobtainable;
		}
		public bool CheckItemObtainability {
			get => default;
			set {
				if (value) {
					List<string> unobtainable = GetUnobtainableItems(Terraria.UI.ItemSlot.ShiftInUse);
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
					string directory = Path.Combine(Program.SavePathShared, "ModSources", nameof(Origins));
					Directory.CreateDirectory(directory);
					string path = Path.Combine(directory, "Unobtainable_Items.txt");
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
	public interface IExpectToBeUnobtainable { }
}
