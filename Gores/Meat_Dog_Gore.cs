using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Gores {
	public class Meat_Dog_Gore1 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 30, 20);
	}
	public class Meat_Dog_Gore2 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 26, 20);
	}
	public class Meat_Dog_Gore3 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 8, 18);
	}
}