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

namespace Origins.Tiles {
    public class Defiled_Stone : ModTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			drop = ItemType<Defiled_Stone_Item>();
			AddMapEntry(new Color(200, 200, 200));
		}
    }
    public class Defiled_Stone_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Stone");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Defiled_Stone>();
		}
    }
}
