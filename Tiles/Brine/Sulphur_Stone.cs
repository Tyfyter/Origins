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

namespace Origins.Tiles.Brine {
    public class Sulphur_Stone : DefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.Stone[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			ItemDrop = ItemType<Sulphur_Stone_Item>();
			AddMapEntry(new Color(18, 73, 56));
            mergeID = TileID.Stone;
		}
        public override bool CanExplode(int i, int j) {
            return false;
        }
    }
    public class Sulphur_Stone_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sulphur Stone");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneBlock);
            Item.createTile = TileType<Sulphur_Stone>();
		}
    }
}
