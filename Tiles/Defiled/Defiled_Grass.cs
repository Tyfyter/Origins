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
            Main.tileMerge[Type][TileID.Dirt] = true;
            Main.tileMerge[TileID.Dirt][Type] = true;
            Main.tileMerge[Type][TileID.Mud] = true;
            Main.tileMerge[TileID.Mud][Type] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                if(TileID.Sets.Grass[i]) {
                    Main.tileMerge[Type][i] = true;
                    Main.tileMerge[i][Type] = true;
                }
            }
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
            drop = ItemID.DirtBlock;
		}
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            fail = true;
            noItem = true;
            Main.tile[i, j].ResetToType(TileID.Dirt);
        }
        /*public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            Main.tile[i, i].type = TileID.CorruptGrass;
            WorldGen.TileFrame(i,j,true,noBreak);
            Main.tile[i, i].type = Type;
            return false;
        }*/
    }
    public class Defiled_Grass_Seeds : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Seeds");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GrassSeeds);
            item.placeStyle = ModContent.TileType<Defiled_Grass>();
		}
        public override bool ConsumeItem(Player player) {
            Main.tile[Player.tileTargetX, Player.tileTargetY].type = (ushort)item.placeStyle;
            return true;
        }
    }
}
