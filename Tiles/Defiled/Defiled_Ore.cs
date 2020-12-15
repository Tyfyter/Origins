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
    public class Defiled_Ore : OriginTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
			drop = ItemType<Defiled_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Lost Ore");
			AddMapEntry(new Color(225, 225, 225), name);
            mergeID = TileID.Demonite;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = g = b = 0.25f;
        }
    }
    public class Defiled_Ore_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Lost Ore");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.DemoniteOre);
            item.createTile = TileType<Defiled_Ore>();
		}
    }
}
