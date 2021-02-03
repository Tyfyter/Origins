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
    public class Defiled_Sandstone_Wall : ModWall {
		public override void SetDefaults() {
            WallID.Sets.Conversion.Sandstone[Type] = true;
            Main.wallBlend[Type] = WallID.Sandstone;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(115, 115, 115));
		}
    }
    public class Defiled_Sandstone_Wall_Safe : Defiled_Sandstone_Wall {
        public override bool Autoload(ref string name, ref string texture) {
            texture = texture.Remove(texture.Length-5);
            return true;
        }
        public override void SetDefaults() {
			drop = ItemType<Defiled_Sandstone_Wall_Item>();
            Main.wallHouse[Type] = true;
            base.SetDefaults();
        }
    }
    public class Defiled_Sandstone_Wall_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Sandstone Wall");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneWall);
            item.createWall = WallType<Defiled_Sandstone_Wall_Safe>();
		}
    }
}
