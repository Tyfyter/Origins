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
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.Stone[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			drop = ItemType<Sulphur_Stone_Item>();
			AddMapEntry(new Color(109, 101, 13));
            mergeID = TileID.Stone;
		}
    }
    public class Sulphur_Stone_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sulphur Stone");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Sulphur_Stone>();
		}
    }
}
