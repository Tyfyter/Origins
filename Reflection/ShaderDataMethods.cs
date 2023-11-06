using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class ShaderDataMethods : ReflectionLoader {
		public override Type ParentType => GetType();
		public static FastFieldInfo<ShaderData, string> _passName;
	}
}