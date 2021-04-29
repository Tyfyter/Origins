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
    //current sprites are just recolored orichalcum ore
    public class Felnum_Ore : OriginTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileMergeDirt[Type] = false;
			drop = ItemType<Felnum_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Felnum Ore");
			AddMapEntry(new Color(160, 116, 42), name);
            mergeID = TileID.Demonite;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if(!Main.tile[i,j].active())return;
            float v = (float)Math.Sin((Main.time-i)/45)*2;
            if(v<0)v=0;
            r = 0.4f-(0.4f*v);
            g = 0.3f+(0.2f*v);
            b = 0.1f+(0.3f*v);
        }
    }
    public class Felnum_Ore_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Felnum Ore");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.DemoniteOre);
            item.createTile = TileType<Felnum_Ore>();
		}
    }
}
