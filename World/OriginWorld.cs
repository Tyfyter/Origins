using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.World {
    public class OriginWorld : ModWorld{
		public static int voidTiles;
		public override void ResetNearbyTileEffects() {
			voidTiles = 0;
		}

		public override void TileCountsAvailable(int[] tileCounts) {
			voidTiles = tileCounts[TileID.AmberGemspark];
		}
    }
}
