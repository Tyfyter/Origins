using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.Gores.NPCs {
	[Autoload(false)]
	public class AutoLoadGores : ModGore {

		private readonly string name;
		public override string Name => name;
		public override string Texture => name;
		AutoLoadGores(string name) {
			this.name = name;
		}
		public static void AddGore(string name, Mod mod) {
			typeof(ILoadable).GetMethod("Load").Invoke(new AutoLoadGores(name), new object[] { mod });
		}
	}
}
