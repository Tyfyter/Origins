using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.World {
    public class OriginWorld : ModWorld {
		public static int voidTiles;
        public int peatSold;

        public override void Load(TagCompound tag) {
            peatSold = tag.GetAsInt("peatSold");
        }
        public override TagCompound Save() {
            return new TagCompound() { {"peatSold",  peatSold} };
        }
        public override void ResetNearbyTileEffects() {
			voidTiles = 0;
		}

		public override void TileCountsAvailable(int[] tileCounts) {
			voidTiles = tileCounts[TileID.AmberGemspark];
		}
    }
}
