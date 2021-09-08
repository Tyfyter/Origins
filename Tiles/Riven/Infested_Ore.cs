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
    public class Infested_Ore : OriginTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
			drop = ItemType<Infested_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Infested Ore");
			AddMapEntry(new Color(207, 148, 58), name);
            mergeID = TileID.Crimtane;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = 0.2f;
            g = 0.15f;
            b = 0.06f;
        }
    }
    public class Infested_Ore_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Infested Ore");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.CrimtaneOre);
            item.createTile = TileType<Infested_Ore>();
		}
    }
}
