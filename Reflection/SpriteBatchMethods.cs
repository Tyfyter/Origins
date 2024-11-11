using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics.Shaders;
using PegasusLib;
using PegasusLib.Reflection;

namespace Origins.Reflection {
	public class SpriteBatchMethods : ReflectionLoader {
		public static FastFieldInfo<SpriteBatch, bool> beginCalled;
	}
}