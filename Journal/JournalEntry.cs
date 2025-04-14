using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Journal {
	public abstract class JournalEntry : ModType, ILocalizedModType, IComparable<JournalEntry> {
		public virtual string TextKey => GetType().Name.Replace("_Entry", "");
		public string NameKey { get; private set; }
		public string NameValue => Language.GetTextValue(NameKey);
		public virtual string[] Aliases => [];
		public virtual ArmorShaderData TextShader => null;
		public virtual Color BaseColor => Color.Black;
		//µ will definitely sort it after anything we'll use in a key
		public virtual JournalSortIndex SortIndex => new("µUncategorized", 0);
		public string LocalizationCategory => "Journal";
		protected sealed override void Register() {
			ModTypeLookup<JournalEntry>.Register(this);
			NameKey = $"Mods.{Mod.Name}.{LocalizationCategory}.{TextKey}.Name";
			if (!Language.Exists(NameKey)) {
				string itemName = $"Mods.{Mod.Name}.Items.{TextKey}.DisplayName";
				if (Language.Exists(itemName)) NameKey = itemName;
			}
			Language.GetOrRegister(NameKey, PrettyPrintName);
			Language.GetOrRegister($"Mods.{Mod.Name}.Journal.{TextKey}.Text");
			Language.GetOrRegister($"Mods.{Mod.Name}.Journal.Series.{SortIndex.Series}", () => SortIndex.Series);
			Journal_Registry.Entries ??= [];
			Journal_Registry.Entries.Add(FullName, this);
		}
		internal int GetQueryIndex(string query) {
			IEnumerable<int> indecies = Aliases
				.Select(v => {
					return v.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
				})
				.Prepend(NameValue.IndexOf(query, StringComparison.CurrentCultureIgnoreCase))
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
	}
}
