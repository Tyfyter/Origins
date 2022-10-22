using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Tiles {
    public abstract class OriginTile : ModTile {
        public static List<OriginTile> IDs { get; internal set; }
        public ushort mergeID;
		public override void Load() {
            if (IDs != null) {
                IDs.Add(this);
            } else {
                IDs = new List<OriginTile>() { this };
            }
            mergeID = Type;
        }
    }
    //temp solution
    public interface DefiledTile {}
    public interface RivenTile {}
}
