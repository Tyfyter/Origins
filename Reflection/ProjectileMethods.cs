using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using DelegateMethods = PegasusLib.Reflection.DelegateMethods;

namespace Origins.Reflection {
	public class ProjectileMethods : ILoadable {
		private delegate Point GetScarabBombDigDirectionSnap8_Del();
		private static GetScarabBombDigDirectionSnap8_Del _GetScarabBombDigDirectionSnap8;
		public void Load(Mod mod) {
			_GetScarabBombDigDirectionSnap8 = typeof(Projectile).GetMethod("GetScarabBombDigDirectionSnap8", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<GetScarabBombDigDirectionSnap8_Del>(new Projectile());
		}
		public void Unload() {
			_GetScarabBombDigDirectionSnap8 = null;
		}
		public static Point GetScarabBombDigDirectionSnap8(Projectile projectile) {
			DelegateMethods._target.SetValue(_GetScarabBombDigDirectionSnap8, projectile);
			return _GetScarabBombDigDirectionSnap8();
		}
	}
}