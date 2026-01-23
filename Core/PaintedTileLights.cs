using Origins.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	public class PaintedTileLights : ILoadable {
		void ILoadable.Load(Mod mod) {
			try {
				MonoModHooks.Add(typeof(TileLoader).GetMethod(nameof(TileLoader.ModifyLight)), ModifyLight);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(PaintedTileLights), e)) throw;
			}
		}
		static void ModifyLight(orig_ModifyLight orig, int i, int j, int type, ref float r, ref float g, ref float b) {
			orig(i, j, type, ref r, ref g, ref b);
			if (OriginClientConfig.Instance.DyeLightTiles) GlowingTileExtensions.PaintLight(ref r, ref g, ref b, Main.tile[i, j].TileColor);
		}
		delegate void orig_ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
		void ILoadable.Unload() {
			throw new NotImplementedException();
		}
	}
}
