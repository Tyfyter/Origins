using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Journal {
	public abstract class JournalEntry : ModType {
		public abstract string TextKey { get; }
		public virtual ArmorShaderData TextShader => null;
		protected sealed override void Register() {
			ModTypeLookup<JournalEntry>.Register(this);
			if (Journal_Registry.Entries is null) Journal_Registry.Entries = new Dictionary<string, JournalEntry>();
			Journal_Registry.Entries.Add(TextKey, this);
		}
	}
}
