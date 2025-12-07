using System;
using Terraria.Graphics.Shaders;
using PegasusLib;
using PegasusLib.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria;
using Terraria.GameContent.Golf;

namespace Origins.Reflection {
	public class FancyGolfPredictionLineMethods : ReflectionLoader {
		public static FastFieldInfo<FancyGolfPredictionLine, Color[]> _colors;
	}
}