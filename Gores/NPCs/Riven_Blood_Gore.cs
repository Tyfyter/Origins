using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static Origins.OriginExtensions;
using Terraria.DataStructures;
using Tyfyter.Utils;

namespace Origins.Gores.NPCs {
	public class R_Effect_Blood1 : ModGore {
		public override Color? GetAlpha(Gore gore, Color lightColor) {
			return new Color(255, 255, 255, 0);
		}
	}
	public class R_Effect_Blood2 : ModGore {
		public override Color? GetAlpha(Gore gore, Color lightColor) {
			return new Color(255, 255, 255, 0);
		}
	}
	public class R_Effect_Blood3 : ModGore {
		public override Color? GetAlpha(Gore gore, Color lightColor) {
			return new Color(255, 255, 255, 0);
		}
	}
}
