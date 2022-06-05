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
    public class Riven_Flesh_Wall : ModWall {
		public override void SetStaticDefaults() {
            WallID.Sets.Conversion.Stone[Type] = true;
            Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(160, 100, 80));
		}
    }
    public class Riven_Flesh_Wall_Safe : Defiled_Stone_Wall {
        public override string Texture => "Origins/Walls/Defiled_Stone_Wall";
        public override void SetStaticDefaults() {
			ItemDrop = ItemType<Riven_Flesh_Wall_Item>();
            Main.wallHouse[Type] = true;
            base.SetStaticDefaults();
        }
    }
    public class Riven_Flesh_Wall_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Flesh Wall");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneWall);
            Item.createWall = WallType<Riven_Flesh_Wall_Safe>();
		}
    }
}
