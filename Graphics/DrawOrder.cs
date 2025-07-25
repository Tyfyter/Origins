using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Origins.Graphics {
	public class DrawOrder : ILoadable {
		public static RenderLayers LastDrawnOverlayLayer { get; private set; }
		void ILoadable.Load(Mod mod) {
			On_OverlayManager.Draw += On_OverlayManager_Draw;
		}

		static void On_OverlayManager_Draw(On_OverlayManager.orig_Draw orig, OverlayManager self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch) {
			orig(self, spriteBatch, layer, beginSpriteBatch);
			LastDrawnOverlayLayer = layer;
		}

		void ILoadable.Unload() { }
	}
}
