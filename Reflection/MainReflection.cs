using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class MainReflection : ILoadable {
		public static FastFieldInfo<Main, int> bgLoops;
		public static int Instance_bgLoops { get => bgLoops.GetValue(Main.instance); set => bgLoops.SetValue(Main.instance, value); }
		public static FastFieldInfo<Main, int> bgStartX;
		public static int Instance_bgStartX { get => bgStartX.GetValue(Main.instance); set => bgStartX.SetValue(Main.instance, value); }
		public static FastFieldInfo<Main, int> bgTopY;
		public static int Instance_bgTopY { get => bgTopY.GetValue(Main.instance); set => bgTopY.SetValue(Main.instance, value); }
		static FastStaticFieldInfo<Main, int> _bgWidthScaled;
		public static int bgWidthScaled { get => _bgWidthScaled.GetValue(); set => _bgWidthScaled.SetValue(value); }
		static FastStaticFieldInfo<Main, float> _bgScale;
		public static float bgScale { get => _bgScale.GetValue(); set => _bgScale.SetValue(value); }
		static FastStaticFieldInfo<Main, Color> _ColorOfSurfaceBackgroundsModified;
		public static Color ColorOfSurfaceBackgroundsModified { get => _ColorOfSurfaceBackgroundsModified.GetValue(); set => _ColorOfSurfaceBackgroundsModified.SetValue(value); }
		public static FastFieldInfo<Main, float> scAdj;
		public static float Instance_scAdj { get => scAdj.GetValue(Main.instance); set => scAdj.SetValue(Main.instance, value); }
		public static FastFieldInfo<Main, float> screenOff;
		public static float Instance_screenOff { get => screenOff.GetValue(Main.instance); set => screenOff.SetValue(Main.instance, value); }
		public static Action<Projectile> DrawProj_Flamethrower { get; private set; }
		public void Load(Mod mod) {
			bgLoops = new("bgLoops", BindingFlags.NonPublic);
			bgStartX = new("bgStartX", BindingFlags.NonPublic);
			bgTopY = new("bgTopY", BindingFlags.NonPublic);
			_bgWidthScaled = new("bgWidthScaled", BindingFlags.NonPublic);
			_bgScale = new("bgScale", BindingFlags.NonPublic);
			_ColorOfSurfaceBackgroundsModified = new("ColorOfSurfaceBackgroundsModified", BindingFlags.NonPublic);
			scAdj = new("scAdj", BindingFlags.NonPublic);
			screenOff = new("screenOff", BindingFlags.NonPublic);
			DrawProj_Flamethrower = typeof(Main).GetMethod(nameof(DrawProj_Flamethrower), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).CreateDelegate<Action<Projectile>>();
		}
		public void Unload() {
			bgLoops = null;
			bgStartX = null;
			bgTopY = null;
			_bgWidthScaled = null;
			_bgScale = null;
			_ColorOfSurfaceBackgroundsModified = null;
			scAdj = null;
			screenOff = null;
			DrawProj_Flamethrower = null;
		}
	}
}