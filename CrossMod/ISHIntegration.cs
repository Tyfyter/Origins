using Origins.Core;
using Origins.Dev;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod {
	internal class ISHIntegration : LateLoadable {
		const string mod_name = "ItemSourceHelper";
		Mod itemSourceHelper;
		List<(string name, HashSet<string> items)> versionTags = [];
		public void SetupVersionTags() {
			versionTags.Clear();
			List<string> names = Mod.GetFileNames();
			Regex versionRegex = new("Info(?:/|\\\\)(\\d+(?:\\.\\d+){0,3})_Items\\.txt", RegexOptions.Compiled);
			string basePath = Path.Combine(Program.SavePathShared, "ModSources", "Origins", "Info");
			if (Directory.Exists(basePath)) {
				foreach (string item in Directory.EnumerateFiles(basePath)) {
					if (versionRegex.Match(item) is { Success: true } match) {
						versionTags.Add((match.Groups[1].Value, File.ReadAllLines(item).ToHashSet()));
					}
				}
			} else {
				for (int i = 0; i < names.Count; i++) {
					if (versionRegex.Match(names[i]) is { Success: true } match) {
						versionTags.Add((match.Groups[1].Value, Encoding.UTF8.GetString(Mod.GetFileBytes(names[i])).Replace("\r", "").Split('\n').ToHashSet()));
					}
				}
			}
		}
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.TryGetMod(mod_name, out itemSourceHelper);
		public override void Load() {
			itemSourceHelper.Call("AddIconicWeapon", DamageClasses.Explosive.Type, (int)ItemID.Bomb);
			itemSourceHelper.Call("AddShimmerFakeCondition", RecipeConditions.ShimmerTransmutation);
			SetupVersionTags();
			itemSourceHelper.Call("AddItemTagProvider", (Func<Item, IEnumerable<string>>)(item => {
				if (item?.ModItem?.Mod is not Origins) return [];
				bool DoSelect((string name, HashSet<string> items) value, out string result) {
					result = $"{nameof(Origins)}_{value.name}";
					return value.items.Contains(item?.ModItem?.Name);
				}
				return versionTags.TrySelect<(string name, HashSet<string> items), string>(DoSelect);
			}));
			itemSourceHelper.Call("AddItemTagProvider", (Func<Item, IEnumerable<string>>)(item => (ItemCategories.Categories[item.type] ?? []).Where(IsWikiCategoryTagFriendly)));
		}
		/// <summary>
		/// Categories which are already a filter or would otherwise be pointless to include
		/// </summary>
		static bool IsWikiCategoryTagFriendly(string tag) => tag switch {
			WikiCategories.Item => false,
			WikiCategories.Potion => false,
			WikiCategories.Food => false,
			WikiCategories.BossSummon => false,
			WikiCategories.Weapon => false,
			WikiCategories.Tool => false,
			WikiCategories.ArmorSet => false,
			WikiCategories.Armor => false,
			WikiCategories.Tile => false,
			WikiCategories.Wall => false,
			WikiCategories.Accessory => false,
			WikiCategories.Ammo => false,
			WikiCategories.Arrow => false,
			WikiCategories.Bullet => false,
			WikiCategories.Dart => false,
			WikiCategories.Canistah => false,
			WikiCategories.Rocket => false,
			WikiCategories.Slug => false,
			WikiCategories.Harpoon => false,
			WikiCategories.RasterSource => false,
			WikiCategories.ToxicSource => false,
			WikiCategories.Torn => false,
			WikiCategories.TornSource => false,
			WikiCategories.Pet => false,
			WikiCategories.LightPet => false,
			WikiCategories.Hardmode => false,
			WikiCategories.Expert => false,
			WikiCategories.Master => false,
			WikiCategories.ReworkExpected => false,
			WikiCategories.PostMLArmorSet => false,
			WikiCategories.UsesBookcase => false,
			nameof(WeaponTypes.Spear) => false,
			nameof(WeaponTypes.Gun) => false,
			nameof(WeaponTypes.HarpoonGun) => false,
			nameof(WeaponTypes.DartLauncher) => false,
			nameof(WeaponTypes.Handcannon) => false,
			nameof(WeaponTypes.RocketLauncher) => false,
			nameof(WeaponTypes.CanisterLauncher) => false,
			_ => true
		};
	}
}
