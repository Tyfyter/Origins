﻿using Microsoft.Xna.Framework;
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
    public class Bleeding_Obsidian : OriginTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			drop = ItemType<Bleeding_Obsidian_Item>();
			AddMapEntry(new Color(57, 10, 75));
		}

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            float m = 0.1f;
            r = 37.2f*m;
            g = 6.7f*m;
            b = 49.2f*m;
        }
    }
    public class Bleeding_Obsidian_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bleeding Obsidian");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneBlock);
            item.createTile = TileType<Bleeding_Obsidian>();
		}
    }
}