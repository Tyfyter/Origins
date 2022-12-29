using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Questing {
	public abstract class Quest : ModType {
		protected sealed override void Register() {
			ModTypeLookup<Quest>.Register(this);
		}
	}
}
