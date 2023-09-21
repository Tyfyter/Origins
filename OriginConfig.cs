using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Reflection;
using ReLogic.OS;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
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

							string filename = item.ModItem.Name + ".json";
							string path = Path.Combine(StatJSONPath, filename);
							File.WriteAllText(path, JsonConvert.SerializeObject(GetWikiStats(item), Formatting.Indented));
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
					Item item = ContentSamples.ItemsByType[value.Type];

					string filename = (item.ModItem?.Name ?? ItemID.Search.GetName(value.Type)) + ".json";
					string path = Path.Combine(StatJSONPath, filename);
					File.WriteAllText(path, JsonConvert.SerializeObject(GetWikiStats(item), Formatting.Indented));
				}
			}
		}
		public ItemDefinition ExportItemPage {
			get => default;
			set {
				if ((value?.Type ?? 0) > ItemID.None && !string.IsNullOrWhiteSpace(WikiTemplatePath) && !string.IsNullOrWhiteSpace(WikiPagePath)) {
					WikiPageExporter.ExportItemPage(ContentSamples.ItemsByType[value.Type]);
				}
			}
		}
		public string WikiTemplatePath { get; set; }
		public string WikiPagePath { get; set; }
		static void AppendStat<T>(JObject data, string name, T value, T defaultValue) {
			if (!value.Equals(defaultValue)) {
				data.Add(name, JToken.FromObject(value));
			}
		}
		internal static JObject GetWikiStats(Item item) {
			JObject data = new();
			ICustomWikiStat customStat = item.ModItem as ICustomWikiStat;
			if (item.ModItem is null) {
				data["Image"] = "MissingTexture";
			} else {
				data["Image"] = $"{item.ModItem.Texture.Replace(nameof(Origins), "§ModImage§")}";
			}
			JArray types = new("Item");
			if (item.accessory) types.Add("Accessory");
			if (item.damage > 0 && item.useStyle != ItemUseStyleID.None) {
				types.Add("Weapon");
				if (!item.noMelee && item.useStyle == ItemUseStyleID.Swing) {
					types.Add("Sword");
				}
				if (item.shoot > ProjectileID.None) {
					switch (ContentSamples.ProjectilesByType[item.shoot].aiStyle) {
						case ProjAIStyleID.Boomerang:
						types.Add("Boomerang");
						break;

						case ProjAIStyleID.Spear:
						types.Add("Spear");
						break;
					}
				}
				switch (item.useAmmo) {
					case ItemID.WoodenArrow:
					types.Add("Bow");
					break;
					case ItemID.MusketBall:
					types.Add("Gun");
					break;
				}
				switch (item.ammo) {
					case ItemID.WoodenArrow:
					types.Add("Arrow");
					break;
					case ItemID.MusketBall:
					types.Add("Bullet");
					break;
				}
			}
			if (customStat?.Hardmode ?? (!item.material && !item.consumable && item.rare > ItemRarityID.Orange)) types.Add("Hardmode");

			if (item.ammo != 0) types.Add("Ammo");
			if (item.pick != 0 || item.axe != 0 || item.hammer != 0 || item.fishingPole != 0 || item.bait != 0) types.Add("Tool");
			if (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1) types.Add("Armor");
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			data.Add("Types", types);

			AppendStat(data, "PickPower", item.pick, 0);
			AppendStat(data, "AxePower", item.axe, 0);
			AppendStat(data, "HammerPower", item.hammer, 0);
			AppendStat(data, "FishPower", item.fishingPole, 0);
			AppendStat(data, "BaitPower", item.bait, 0);

			if (item.createTile != -1) {
				ModTile tile = TileLoader.GetTile(item.createTile);
				if (tile is not null) {
					AppendStat(data, Main.tileHammer[item.createTile] ? "HammerReq" : "PickReq", tile.MinPick, 0);
				}
				int width = 1, height = 1;
				if (TileObjectData.GetTileData(item.createTile, item.placeStyle) is TileObjectData tileData) {
					width = tileData.Width;
					height = tileData.Height;
				}
				data.Add("PlacementSize", new JArray(width, height));
			}
			AppendStat(data, "Defense", item.defense, 0);
			if (item.headSlot != -1) {
				data.Add("ArmorSlot", "Helmet");
			} else if (item.bodySlot != -1) {
				data.Add("ArmorSlot", "Shirt");
			} else if (item.legSlot != -1) {
				data.Add("ArmorSlot", "Pants");
			}
			AppendStat(data, "ManaCost", item.mana, 0);
			AppendStat(data, "HealLife", item.healLife, 0);
			AppendStat(data, "HealMana", item.healMana, 0);
			AppendStat(data, "Damage", item.damage, -1);
			if (item.damage > 0) {
				string damageClass = item.DamageType.DisplayName.Value;
				damageClass = damageClass.Replace(" damage", "");
				damageClass = System.Text.RegularExpressions.Regex.Replace(damageClass, "( |^)(\\w)", (match) => match.Groups[2].Value.ToUpper());
				data.Add("DamageClass", damageClass);
			}
			AppendStat(data, "Knockback", item.knockBack, 0);
			AppendStat(data, "Crit", item.crit + 4, 4);
			AppendStat(data, "UseTime", item.useTime, 100);
			AppendStat(data, "Velocity", item.shootSpeed, 0);
			string itemTooltip = "";
			for (int i = 0; i < item.ToolTip.Lines; i++) {
				if (i > 0) itemTooltip += "\n";
				itemTooltip += item.ToolTip.GetLine(i);
			}
			AppendStat(data, "Tooltip", itemTooltip, "");
			AppendStat(data, "Rarity", (RarityLoader.GetRarity(item.rare)?.Name ?? ItemRarityID.Search.GetName(item.rare)).Replace("Rarity", ""), "");
			if (customStat?.Buyable ?? false) AppendStat(data, "Buy", item.value, 0);
			AppendStat(data, "Sell", item.value / 5, 0);

			if (customStat is not null) customStat.ModifyWikiStats(data);
			AppendStat(data, "SpriteWidth", item.ModItem is null ? item.width : ModContent.Request<Texture2D>(item.ModItem.Texture).Width(), 0);
			
			return data;
		}
	}
	internal interface ICustomWikiStat {
		bool Buyable => false;
		void ModifyWikiStats(JObject data) {}
		string[] Categories => Array.Empty<string>();
		bool? Hardmode => null;
	}
}
