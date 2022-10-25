using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Journal {
	public abstract class JournalEntry : ModType {
		public abstract string TextKey { get; }
		public string NameKey { get; private set; }
		public new string Name => Language.GetTextValue(NameKey);
		public virtual string[] Aliases => Array.Empty<string>();
		public virtual ArmorShaderData TextShader => null;
		protected sealed override void Register() {
			ModTypeLookup<JournalEntry>.Register(this);
			NameKey = Language.Exists("Mods.Origins.Journal.Name." + TextKey) ? "Mods.Origins.Journal.Name." + TextKey : "Mods.Origins.ItemName." + TextKey;
			if (Journal_Registry.Entries is null) Journal_Registry.Entries = new Dictionary<string, JournalEntry>();
			Journal_Registry.Entries.Add(TextKey, this);
		}
		internal int GetQueryIndex(string query) {
			var indecies = Aliases
				.Select(v => {
					return v.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
				})
				.Prepend(Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase))
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
