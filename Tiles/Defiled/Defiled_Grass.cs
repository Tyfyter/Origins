using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Defiled {
    public class Defiled_Grass : ModTile {
		public override void SetDefaults() {
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.NeedsGrassFraming[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
		}
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            fail = true;
            noItem = true;
            Main.tile[i, j].ResetToType(TileID.Dirt);
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            return true;
        }
    }
}
