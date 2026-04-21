#pragma warning disable CS0649
#pragma warning disable IDE0079
#pragma warning disable IDE0044
using Terraria;
using Terraria.Graphics.Light;
using PegasusLib.Reflection;

namespace Origins.Reflection {
	public class LightingMethods : ReflectionLoader {
		[ReflectionParentType(typeof(LegacyLighting))]
		public static FastFieldInfo<LegacyLighting, float> _blueWave;
		[ReflectionParentType(typeof(Lighting))]
		public static FastStaticFieldInfo<Lighting, ILightingEngine> _activeEngine;
		public static FastFieldInfo<LightingEngine, Rectangle> _activeProcessedArea;
		public static FastFieldInfo<LightingEngine, LightMap> _activeLightMap;
		public static FastFieldInfo<LightMap, Vector3[]> _colors;
	}
}
