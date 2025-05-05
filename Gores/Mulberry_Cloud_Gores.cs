using System.Collections.Generic;
using Terraria.ModLoader;

namespace Origins.Gores {
	public class Mulberry_Cloud_1 : ModGore {
		public static List<ModGore> gores = [];
		public override void SetStaticDefaults() {
			UpdateType = 11;
			gores.Add(this);
		}
		public override void Unload() {
			gores = null;
		}
	}
	public class Mulberry_Cloud_2 : ModGore {
		public override void SetStaticDefaults() {
			UpdateType = 12;
			Mulberry_Cloud_1.gores.Add(this);
		}
	}
	public class Mulberry_Cloud_3 : ModGore {
		public override void SetStaticDefaults() {
			UpdateType = 13;
			Mulberry_Cloud_1.gores.Add(this);
		}
	}
}
