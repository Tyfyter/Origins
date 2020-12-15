using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.Tiles {
    public abstract class OriginTile : ModTile {
        public static List<OriginTile> IDs;
        public ushort mergeID;
        public override bool Autoload(ref string name, ref string texture) {
            if(IDs!=null) {
                IDs.Add(this);
            } else {
                IDs = new List<OriginTile>() {this};
            }
            mergeID = Type;
            return true;
        }
    }
}
