using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Endowood_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Wood;
			AddMapEntry(new Color(30, 10, 30));
			Main.wallHouse[Type] = true;
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Wall_Item : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Endowood_Wall>());
		}
	}
}
