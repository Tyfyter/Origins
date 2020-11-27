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
    public class Defiled_Ore : ModTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			drop = ItemType<Defiled_Ore_Item>();
			AddMapEntry(new Color(225, 225, 225));
		}
    }
    public class Defiled_Ore_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Lost Ore");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Defiled_Ore>();
		}
    }
}
