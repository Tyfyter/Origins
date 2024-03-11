using System;
using Terraria.Graphics.Shaders;

namespace Origins.Reflection {
	public class ShaderDataMethods : ReflectionLoader {
		public override Type ParentType => GetType();
		public static FastFieldInfo<ShaderData, string> _passName;
	}
}