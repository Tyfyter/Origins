using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
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
    public class Defiled_Stone : DefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.Stone[Type] = true;
            TileID.Sets.Conversion.Stone[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
            Main.tileMerge[Type][TileID.Stone] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
            }*/
			ItemDrop = ItemType<Defiled_Stone_Item>();
			AddMapEntry(new Color(200, 200, 200));
			//SetModTree(Defiled_Tree.Instance);
            mergeID = TileID.Stone;
            MinPick = 65;
            MineResist = 2;
		}
        public override bool CreateDust(int i, int j, ref int type) {
            type = Defiled_Wastelands.DefaultTileDust;
            return true;
        }
    }
    public class Defiled_Stone_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Stone");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneBlock);
            Item.createTile = TileType<Defiled_Stone>();
		}
    }
}
