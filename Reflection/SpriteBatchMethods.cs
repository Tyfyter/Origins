using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics.Shaders;

namespace Origins.Reflection {
	public class SpriteBatchMethods : ReflectionLoader {
		public override Type ParentType => GetType();
		public static FastFieldInfo<SpriteBatch, bool> beginCalled;
	}
}