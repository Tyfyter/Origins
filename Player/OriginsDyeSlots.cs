using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins {
	public class OriginsDyeSlots : ModPlayer {
		public int? cSceneMYKDye0;
		public int? cSceneMYKDye1;
		public int? cSceneMYKDye2;
		public int? cFuturephonesGlow;
		public override void ResetEffects() {
			cSceneMYKDye0 = null;
			cSceneMYKDye1 = null;
			cSceneMYKDye2 = null;
			cFuturephonesGlow = null;
		}
	}
}
