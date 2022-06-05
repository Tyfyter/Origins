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
    public class Defiled_Ice : DefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
		    TileID.Sets.Ices[Type] = true;
		    TileID.Sets.IcesSlush[Type] = true;
		    TileID.Sets.IcesSnow[Type] = true;
            TileID.Sets.Conversion.Ice[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.IceBlock];
            Main.tileMerge[Type][TileID.IceBlock] = true;
			Main.tileBlockLight[Type] = true;
			ItemDrop = ItemType<Defiled_Ice_Item>();
			AddMapEntry(new Color(225, 225, 225));
            mergeID = TileID.IceBlock;
		}
        public override bool CreateDust(int i, int j, ref int type) {
            type = DefiledWastelands.DefaultTileDust;
            return true;
        }
        public override void FloorVisuals(Player player) {
            player.slippy = true;
        }
    }
    public class Defiled_Ice_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Ice");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneBlock);
            Item.createTile = TileType<Defiled_Ice>();
		}
    }
}
