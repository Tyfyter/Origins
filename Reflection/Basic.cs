using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class Basic : ILoadable {
		public static FastFieldInfo<Delegate, object> _target;
		public void Load(Mod mod) {
			_target = new("_target", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		public void Unload() {
			_target = null;
		}
	}
}