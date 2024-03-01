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
		}
	}
	public class Endowood_Wall_Item : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodWall);
			Item.createWall = WallType<Endowood_Wall>();
		}
	}
}
