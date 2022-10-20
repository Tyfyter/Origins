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

namespace Origins.Walls {
    public class Hardened_Defiled_Sand_Wall : ModWall {
		public override void SetStaticDefaults() {
            WallID.Sets.Conversion.HardenedSand[Type] = true;
            Main.wallBlend[Type] = WallID.HardenedSand;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(150, 150, 150));
		}
    }
    public class Hardened_Defiled_Sand_Wall_Safe : Hardened_Defiled_Sand_Wall {
        public override string Texture => "Origins/Walls/Hardened_Defiled_Sand_Wall";
        public override void SetStaticDefaults() {
			ItemDrop = ItemType<Hardened_Defiled_Sand_Wall_Item>();
            Main.wallHouse[Type] = true;
            base.SetStaticDefaults();
        }
    }
    public class Hardened_Defiled_Sand_Wall_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Hardened {$Defiled} Sand Wall");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneWall);
            Item.createWall = WallType<Hardened_Defiled_Sand_Wall_Safe>();
		}
    }
}
