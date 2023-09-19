using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
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
							File.WriteAllText(path, GetWikiStats(item));
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
					File.WriteAllText(path, GetWikiStats(item));
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
		static void AppendLine(ref string data, string newData) {
			if (data.Length > 2) {
				data += $",\n";
			}
			data += "\t" + newData;
		}
		static void AppendStat<T>(ref string data, string name, T value, T defaultValue) {
			if (!value.Equals(defaultValue)) {
				string text = $"\"{name}\":";
				if (value is string) {
					text += $"\"{value}\"";
				} else if (value is float) {
					text += $"{value:0.##}";
				} else {
					text += value.ToString();
				}
				AppendLine(ref data, text);
			}
		}
		internal static string GetWikiStats(Item item) {
			string data = "{\n";
			ICustomWikiStat customStat = item.ModItem as ICustomWikiStat;
			if (item.ModItem is null) {
				data += $"\t\"Image\": \"MissingTexture\"";
			} else {
				data += $"\t\"Image\": \"{item.ModItem.Texture.Replace(nameof(Origins), "§ModImage§")}\"";
			}

			string types = ",\n\t\"Types\":[\n\t\t\"Item\"";
			if (item.accessory) AppendLine(ref types, "\t\"Accessory\"");
			if (item.damage > 0 && item.useStyle != ItemUseStyleID.None) {
				AppendLine(ref types, "\t\"Weapon\"");
				if (!item.noMelee && item.useStyle == ItemUseStyleID.Swing) {
					AppendLine(ref types, "\t\"Sword\"");
				}
				if (item.shoot > ProjectileID.None) {
					switch (ContentSamples.ProjectilesByType[item.shoot].aiStyle) {
						case ProjAIStyleID.Boomerang:
						AppendLine(ref types, "\t\"Boomerang\"");
						break;

						case ProjAIStyleID.Spear:
						AppendLine(ref types, "\t\"Spear\"");
						break;
					}
				}
				switch (item.useAmmo) {
					case ItemID.WoodenArrow:
					AppendLine(ref types, "\t\"Bow\"");
					break;
					case ItemID.MusketBall:
					AppendLine(ref types, "\t\"Gun\"");
					break;
				}
				switch (item.ammo) {
					case ItemID.WoodenArrow:
					AppendLine(ref types, "\t\"Arrow\"");
					break;
					case ItemID.MusketBall:
					AppendLine(ref types, "\t\"Bullet\"");
					break;
				}
			}
			if (customStat?.Hardmode ?? (!item.material && !item.consumable && item.rare > ItemRarityID.Orange)) AppendLine(ref types, "\t\"Hardmode\"");

			if (item.ammo != 0) AppendLine(ref types, "\t\"Ammo\"");
			if (item.pick != 0 || item.axe != 0 || item.hammer != 0 || item.fishingPole != 0 || item.bait != 0) AppendLine(ref types, "\t\"Tool\"");
			if (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1) AppendLine(ref types, "\t\"Armor\"");
			if (customStat is not null) foreach (string cat in customStat.Categories) AppendLine(ref types, $"\t\"{cat}\"");
			types += "\n\t]";
			data += types;

			AppendStat(ref data, "PickPower", item.pick, 0);
			AppendStat(ref data, "AxePower", item.axe, 0);
			AppendStat(ref data, "HammerPower", item.hammer, 0);
			AppendStat(ref data, "FishPower", item.fishingPole, 0);
			AppendStat(ref data, "BaitPower", item.bait, 0);

			if (item.createTile != -1) {
				ModTile tile = TileLoader.GetTile(item.createTile);
				if (tile is not null) {
					AppendStat(ref data, Main.tileHammer[item.createTile] ? "HammerReq" : "PickReq", tile.MinPick, 0);
				}
				int width = 1, height = 1;
				if (TileObjectData.GetTileData(item.createTile, item.placeStyle) is TileObjectData tileData) {
					width = tileData.Width;
					height = tileData.Height;
				}
				AppendLine(ref data, $"\"PlacementSize\": [{width}, {height}]");
			}
			AppendStat(ref data, "Defense", item.defense, 0);
			if (item.headSlot != -1) {
				AppendLine(ref data, "\"ArmorSlot\":\"Helmet\"");
			} else if (item.bodySlot != -1) {
				AppendLine(ref data, "\"ArmorSlot\":\"Shirt\"");
			} else if (item.legSlot != -1) {
				AppendLine(ref data, "\"ArmorSlot\":\"Pants\"");
			}
			AppendStat(ref data, "ManaCost", item.mana, 0);
			AppendStat(ref data, "HealLife", item.healLife, 0);
			AppendStat(ref data, "HealMana", item.healMana, 0);
			AppendStat(ref data, "Damage", item.damage, -1);
			if (item.damage > 0) {
				string damageClass = item.DamageType.DisplayName.Value;
				damageClass = damageClass.Replace(" damage", "");
				damageClass = System.Text.RegularExpressions.Regex.Replace(damageClass, "( |^)(\\w)", (match) => match.Groups[2].Value.ToUpper());
				AppendLine(ref data, $"\"DamageClass\":\"{damageClass}\"");
			}
			AppendStat(ref data, "Knockback", item.knockBack, 0);
			AppendStat(ref data, "Crit", item.crit + 4, 4);
			AppendStat(ref data, "UseTime", item.useTime, 100);
			AppendStat(ref data, "Velocity", item.shootSpeed, 0);
			string itemTooltip = "";
			for (int i = 0; i < item.ToolTip.Lines; i++) {
				if (i > 0) itemTooltip += "\n";
				itemTooltip += item.ToolTip.GetLine(i).Replace(",", "\\\\,").Replace("'", "\\\\'");
			}
			AppendStat(ref data, "Tooltip", itemTooltip.Replace("\n", "\\n"), "");
			AppendStat(ref data, "Rarity", (RarityLoader.GetRarity(item.rare)?.Name ?? ItemRarityID.Search.GetName(item.rare)).Replace("Rarity", ""), "");
			if (customStat?.Buyable ?? false) AppendStat(ref data, "Buy", item.value, 0);
			AppendStat(ref data, "Sell", item.value / 5, 0);

			if (customStat?.WikiStats is not null) AppendLine(ref data, customStat.WikiStats);
			AppendStat(ref data, "SpriteWidth", item.ModItem is null ? item.width :ModContent.Request<Texture2D>(item.ModItem.Texture).Width(), 0);
			data += "\n}";
			return data;
		}
	}
	internal interface ICustomWikiStat {
		bool Buyable => false;
		string WikiStats => null;
		string[] Categories => Array.Empty<string>();
		bool? Hardmode => null;
	}
}
