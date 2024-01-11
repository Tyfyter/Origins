using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Quartz_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.Conversion.Sandstone[Type] = true;
			Main.wallBlend[Type] = WallID.Sandstone;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(115, 115, 115));
		}
	}
	public class Quartz_Wall_Safe : Brittle_Quartz_Wall {
		public override string Texture => "Origins/Walls/Quartz_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Quartz_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Quartz_Wall_Safe>();
		}
	}
}
