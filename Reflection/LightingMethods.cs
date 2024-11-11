#pragma warning disable CS0649
#pragma warning disable IDE0044
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Light;
using PegasusLib;
using PegasusLib.Reflection;

namespace Origins.Reflection {
	public class LightingMethods : ReflectionLoader {
		[ReflectionParentType(typeof(LegacyLighting))]
		public static FastFieldInfo<LegacyLighting, float> _blueWave;
		[ReflectionParentType(typeof(Lighting))]
		public static FastStaticFieldInfo<Lighting, ILightingEngine> _activeEngine;
	}
}
