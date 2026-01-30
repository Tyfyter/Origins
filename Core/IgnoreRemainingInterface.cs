using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace Origins.Core {
	//TODO: move to PegasusLib
	internal class IgnoreRemainingInterface : ModSystem {
		public static void Activate() => active = true;
		static bool active;
		public override void Load() {
			MonoModHooks.Add(typeof(PlayerInput).GetProperty(nameof(PlayerInput.IgnoreMouseInterface)).GetGetMethod(), (orig_IgnoreMouseInterface orig) => {
				return active || orig();
			});
		}
		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			active = false;
		}
		delegate bool orig_IgnoreMouseInterface();
	}
}
