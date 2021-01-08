using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
    public class Defiled_Sandstone : DefiledTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sandstone];
            TileID.Sets.Conversion.Sandstone[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
            //Main.tileMerge[TileID.Sandstone][Type] = true;
            //Main.tileMerge[Type] = Main.tileMerge[TileID.Sandstone];
            //Main.tileMerge[Type][TileID.Sandstone] = true;
            /*for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Sandstone];
            }*/
			drop = ItemType<Defiled_Sandstone_Item>();
			AddMapEntry(new Color(150, 150, 150));
            mergeID = TileID.Sandstone;
		}
    }
    public class Defiled_Sandstone_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Sandstone");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Defiled_Sandstone>();
		}
    }
}
