using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class MainReflection : ILoadable {
		public static FastFieldInfo<Main, int> bgLoops;
		public static FastFieldInfo<Main, int> bgStartX;
		public static FastFieldInfo<Main, int> bgTopY;
		static FastStaticFieldInfo<Main, int> _bgWidthScaled;
		public static int bgWidthScaled { get => _bgWidthScaled.GetValue(); set => _bgWidthScaled.SetValue(value); }
		static FastStaticFieldInfo<Main, float> _bgScale;
		public static float bgScale { get => _bgScale.GetValue(); set => _bgScale.SetValue(value); }
		static FastStaticFieldInfo<Main, Color> _ColorOfSurfaceBackgroundsModified;
		public static Color ColorOfSurfaceBackgroundsModified { get => _ColorOfSurfaceBackgroundsModified.GetValue(); set => _ColorOfSurfaceBackgroundsModified.SetValue(value); }
		public void Load(Mod mod) {
			bgLoops = new("bgLoops", BindingFlags.NonPublic);
			bgStartX = new("bgStartX", BindingFlags.NonPublic);
			bgTopY = new("bgTopY", BindingFlags.NonPublic);
			_bgWidthScaled = new("bgWidthScaled", BindingFlags.NonPublic);
			_bgScale = new("bgScale", BindingFlags.NonPublic);
			_ColorOfSurfaceBackgroundsModified = new("ColorOfSurfaceBackgroundsModified", BindingFlags.NonPublic);
		}
		public void Unload() {
			bgLoops = null;
			bgStartX = null;
			bgTopY = null;
			_bgWidthScaled = null;
			_bgScale = null;
			_ColorOfSurfaceBackgroundsModified = null;
		}
	}
}