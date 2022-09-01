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
		}
	}
}
