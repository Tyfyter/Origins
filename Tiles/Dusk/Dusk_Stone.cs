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

namespace Origins.Tiles.Dusk {
    public class Dusk_Stone : ModTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			drop = ItemType<Dusk_Stone_Item>();
			AddMapEntry(new Color(0, 0, 0));
		}
        public override void PostSetDefaults() {
            Main.tileNoSunLight[Type] = true;
        }
    }
    public class Dusk_Stone_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dusk Stone");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Dusk_Stone>();
		}
    }
}
