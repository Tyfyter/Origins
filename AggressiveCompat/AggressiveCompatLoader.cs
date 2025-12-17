using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria.ModLoader;

namespace Origins.AggressiveCompat {
	internal abstract class AggressiveCompatLoader : ILoadable {
		public abstract bool IsLoadingEnabled(Mod mod);
		public abstract void Load();
		void ILoadable.Load(Mod mod) {
			try {
				Load();
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(GetType().Name, ex)) throw;
			}
		}
		void ILoadable.Unload() { }
		protected static void ILHook(string modName, string typeName, string methodName, ILContext.Manipulator hook) {
			MonoModHooks.Modify(ModLoader.GetMod(modName).Code.GetType(typeName).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance), hook);
		}
	}
}
