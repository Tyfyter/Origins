using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using Origins.World.BiomeData;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Journal {
	public abstract class JournalEntry : ModType, ILocalizedModType, IComparable<JournalEntry> {
		public virtual string TextKey => GetType().Name.Replace("_Entry", "");
		public virtual string FullTextKey => $"{SortIndex.Series}.{TextKey}";
		public virtual LocalizedText DisplayName {
			get {
				string nameKey = $"Mods.{Mod.Name}.{LocalizationCategory}.{FullTextKey}.Name";
				if (!Language.Exists(nameKey)) {
					string itemName = $"Mods.{Mod.Name}.Items.{TextKey}.DisplayName";
					if (Language.Exists(itemName)) {
						nameKey = itemName;
					} else {
						itemName = $"Mods.{Mod.Name}.NPCs.{TextKey}.DisplayName";
						if (Language.Exists(itemName)) nameKey = itemName;
					}
				}
				return Language.GetOrRegister(nameKey, PrettyPrintName);
			}
		}
		public virtual string[] Aliases => [];
		public virtual ArmorShaderData TextShader => null;
		public virtual Color BaseColor => Color.Black;
		//µ will definitely sort it after anything we'll use in a key
		public virtual JournalSortIndex SortIndex => new("µUncategorized", 0);
		public string LocalizationCategory => "Journal";
		protected sealed override void Register() {
			ModTypeLookup<JournalEntry>.Register(this);
			Journal_Registry.Entries ??= [];
			Journal_Registry.Entries.Add(FullName, this);
		}
		public sealed override void SetupContent() {
			Language.GetOrRegister($"Mods.{Mod.Name}.Journal.{FullTextKey}.Text");
			Language.GetOrRegister($"Mods.{Mod.Name}.Journal.{SortIndex.Series}.DisplayName", () => SortIndex.Series);
			_ = DisplayName.Value;
			SetStaticDefaults();
		}
		internal int GetQueryIndex(string query) {
			IEnumerable<int> indecies = Aliases
				.Select(v => {
					return v.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
				})
				.Prepend(DisplayName.Value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase))
				.Where(v => {
					return v >= 0;
				});
			if (!indecies.Any()) {
				return -1;
			}
			return indecies.Min();
		}
		public int CompareTo(JournalEntry other) {
			int compareIndex = SortIndex.CompareTo(other.SortIndex);
			if (compareIndex != 0) return compareIndex;
			return Name.CompareTo(other.Name);
		}
		public readonly record struct JournalSortIndex(string Series, int Part) : IComparable<JournalSortIndex> {
			public readonly int CompareTo(JournalSortIndex other) {
				if (Series != other.Series) return Series.CompareTo(other.Series);
				return Part.CompareTo(other.Part);
			}
		}
		public static void AddJournalEntry(ref string current, string newEntry) {
			if (!string.IsNullOrEmpty(current)) current += ';';
			current += newEntry;
		}
	}
	public abstract class VanillaItemJournalEntry : JournalEntry {
		public abstract int ItemType { get; }
		public override LocalizedText DisplayName => Lang.GetItemName(ItemType);
		public override void SetStaticDefaults() {
			JournalEntry.AddJournalEntry(ref OriginsSets.Items.JournalEntries[ItemType], FullName);
		}
	}
	public abstract class VanillaNPCJournalEntry : JournalEntry {
		public abstract int NPCType { get; }
		public override LocalizedText DisplayName => Lang.GetNPCName(NPCType);
		public override void SetStaticDefaults() {
			JournalEntry.AddJournalEntry(ref OriginsSets.NPCs.JournalEntries[NPCType], FullName);
		}
	}
	public class Journal_Entry_Condition(JournalEntry entry) : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			OriginPlayer oP = info.player.OriginPlayer();
			return oP.journalUnlocked && !oP.unlockedJournalEntries.Contains(entry.FullName);
		}
		public bool CanShowItemDropInUI() {
			return true;
		}
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.JournalEntry").Format(entry.DisplayName);
		}
	}
}
