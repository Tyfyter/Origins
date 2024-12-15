using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Journal {
	public abstract class JournalEntry : ModType {
		public abstract string TextKey { get; }
		public string NameKey { get; private set; }
		public string NameValue => Language.GetTextValue(NameKey);
		public virtual string[] Aliases => [];
		public virtual ArmorShaderData TextShader => null;
		public virtual Color BaseColor => Color.Black;
		protected sealed override void Register() {
			ModTypeLookup<JournalEntry>.Register(this);
			NameKey = $"Mods.{Mod.Name}.Journal.{TextKey}.Name";
			if (!Language.Exists(NameKey)) {
				NameKey = $"Mods.{Mod.Name}.Items.{TextKey}.DisplayName";
			}
			Language.GetOrRegister(NameKey);
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
