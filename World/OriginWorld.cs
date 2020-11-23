using Origins.Items.Weapons.Felnum.Tier2;
using Origins.Tiles;
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
		public static int defiledTiles;
        public int peatSold;
        public const float biomeShaderSmoothing = 0.025f;

        //public bool felnumBroadswordStab = false;

        public override void Load(TagCompound tag) {
            peatSold = tag.GetAsInt("peatSold");
            /*felnumBroadswordStab = tag.GetBool("felnumBroadswordStab");
            if(felnumBroadswordStab)Felnum_Broadsword.animation.Frame = 0;*/
        }
        public override TagCompound Save() {
            return new TagCompound() { {"peatSold",  peatSold} /*, {"felnumBroadswordStab",  felnumBroadswordStab} */};
        }
        public override void ResetNearbyTileEffects() {
			voidTiles = 0;
            defiledTiles = 0;
		}

		public override void TileCountsAvailable(int[] tileCounts) {
			voidTiles = tileCounts[ModContent.TileType<Dusk_Stone>()];
			defiledTiles = tileCounts[ModContent.TileType<Defiled_Stone>()]+tileCounts[ModContent.TileType<Defiled_Grass>()];
		}
    }
}
