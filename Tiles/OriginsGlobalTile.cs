using Origins.Tiles.Defiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles {
    public class OriginsGlobalTile : GlobalTile {
        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
            if(Main.tile[i, j - 1].type == Defiled_Altar.id && type != Defiled_Altar.id)return false;
            return true;
        }
    }
}
