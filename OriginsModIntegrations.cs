using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins {
	public class OriginsModIntegrations : ILoadable {
		private static OriginsModIntegrations instance;
		Mod wikiThis;
		public static Mod WikiThis { get => instance.wikiThis; set => instance.wikiThis = value; }
		public void Load(Mod mod) {
			instance = this;
			if (!Main.dedServ && ModLoader.TryGetMod("Wikithis", out wikiThis)) {
				WikiThis.Call("AddModURL", Origins.instance, "tyfyter.github.io/OriginsWiki");
				Origins.instance.Logger.Info("Added Wikithis integration");
			}
		}

		public void Unload() {
			instance = null;
		}
	}
}
