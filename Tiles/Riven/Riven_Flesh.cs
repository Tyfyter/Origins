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

namespace Origins.Tiles.Riven {
    public class Riven_Flesh : RivenTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.Stone[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
            Main.tileMerge[Type][TileID.Stone] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
            }*/
			drop = ItemType<Riven_Flesh_Item>();
			AddMapEntry(new Color(200, 125, 100));
			//SetModTree(Defiled_Tree.Instance);
            mergeID = TileID.Stone;
            soundType = SoundID.NPCKilled;
		}
    }
    public class Riven_Flesh_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Flesh");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.FleshBlock);
            item.createTile = TileType<Riven_Flesh>();
		}
    }
}
