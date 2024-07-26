using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Marrowick_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Wood;
			AddMapEntry(new Color(130, 110, 90));
			Main.wallHouse[Type] = true;
		}
	}
	public class Marrowick_Wall_Item : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Marrowick_Wall>());
		}
	}
}
