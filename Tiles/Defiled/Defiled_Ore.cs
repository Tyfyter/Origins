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
    public class Defiled_Ore : OriginTile, IComplexMineDamageTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.Ore[Type] = true;
			ItemDrop = ItemType<Defiled_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Lost Ore");
			AddMapEntry(new Color(225, 225, 225), name);
            mergeID = TileID.Demonite;
		}
        public override bool CreateDust(int i, int j, ref int type) {
            type = DustID.WhiteTorch;
            return true;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = g = b = 0.25f;
        }
        public void MinePower(int i, int j, int minePower, ref int damage) {
            if (minePower >= 55 || j <= Main.worldSurface) {
                damage += (int)(minePower / mineResist);
            }
        }
    }
    public class Defiled_Ore_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Lost Ore");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DemoniteOre);
            Item.createTile = TileType<Defiled_Ore>();
		}
    }
}
