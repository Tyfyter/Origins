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
    public class Hardened_Defiled_Sand : DefiledTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.HardenedSand[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.HardenedSand];
            Main.tileMerge[TileID.HardenedSand][Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.HardenedSand];
            Main.tileMerge[Type][TileID.HardenedSand] = true;*/
			drop = ItemType<Hardened_Defiled_Sand_Item>();
			AddMapEntry(new Color(200, 200, 200));
            mergeID = TileID.HardenedSand;
		}
    }
    public class Hardened_Defiled_Sand_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hardened Defiled Sand");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Hardened_Defiled_Sand>();
		}
    }
}
