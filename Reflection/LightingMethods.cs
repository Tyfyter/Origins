using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Light;

namespace Origins.Reflection {
	public class LightingMethods : ReflectionLoader {
		public override Type ParentType => GetType();
		[ReflectionParentType(typeof(LegacyLighting))]
		public static FastFieldInfo<LegacyLighting, float> _blueWave;
		[ReflectionParentType(typeof(Lighting))]
		public static FastStaticFieldInfo<Lighting, ILightingEngine> _activeEngine;
	}
}
