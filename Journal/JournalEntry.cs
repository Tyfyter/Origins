using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Journal {
	public abstract class JournalEntry : ModType, ILocalizedModType {
		public abstract string TextKey { get; }
		public string NameKey { get; private set; }
		public string NameValue => Language.GetTextValue(NameKey);
		public virtual string[] Aliases => [];
		public virtual ArmorShaderData TextShader => null;
		public virtual Color BaseColor => Color.Black;
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
			Journal_Registry.Entries ??= [];
			Journal_Registry.Entries.Add(Mod.Name + "/" + Name, this);
		}
		internal int GetQueryIndex(string query) {
			var indecies = Aliases
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
	}
}
