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
    public class Defiled_Stone_Wall : ModWall {
		public override void SetDefaults() {
            WallID.Sets.Conversion.Stone[Type] = true;
            Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(150, 150, 150));
		}
    }
    public class Defiled_Stone_Wall_Safe : Defiled_Stone_Wall {
        public override bool Autoload(ref string name, ref string texture) {
            texture = texture.Remove(texture.Length-5);
            return true;
        }
        public override void SetDefaults() {
			drop = ItemType<Defiled_Stone_Wall_Item>();
            Main.wallHouse[Type] = true;
            base.SetDefaults();
        }
    }
    public class Defiled_Stone_Wall_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Stone Wall");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.StoneWall);
            item.createWall = WallType<Defiled_Stone_Wall_Safe>();
		}
    }
}
