using Origins.Items.Weapons.Felnum.Tier2;
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
        public bool felnumBroadswordStab = false;

        public override void Load(TagCompound tag) {
            peatSold = tag.GetAsInt("peatSold");
            felnumBroadswordStab = tag.GetBool("felnumBroadswordStab");
            if(felnumBroadswordStab)Felnum_Broadsword.animation.Frame = 0;
        }
        public override TagCompound Save() {
            return new TagCompound() { {"peatSold",  peatSold} , {"felnumBroadswordStab",  felnumBroadswordStab} };
        }
        public override void ResetNearbyTileEffects() {
			voidTiles = 0;
		}

		public override void TileCountsAvailable(int[] tileCounts) {
			voidTiles = tileCounts[TileID.AmberGemspark];
		}
    }
}
