using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins {
	public class OriginsModIntegrations : ILoadable {
		Mod wikiThis;
		public void Load(Mod mod) {
			if (!Main.dedServ && ModLoader.TryGetMod("Wikithis", out wikiThis)) {
				wikiThis.Call("AddModURL", Origins.instance, "tyfyter.github.io/OriginsWiki");
				Origins.instance.Logger.Info("Added Wikithis integration");
			}
		}

		public void Unload() {
		}
	}
}
