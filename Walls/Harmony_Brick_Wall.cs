using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Harmony_Brick_Wall : ModWall {
		public override void SetStaticDefaults() {
			AddMapEntry(new Color(120, 20, 60));
			DustType = DustID.Ice_Red;
		}
	}
	public class Harmony_Brick_Wall_Safe : Harmony_Brick_Wall {
		public override string Texture => "Origins/Walls/Harmony_Brick_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Harmony_Brick_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Harmony_Brick_Wall_Safe>());
		}
	}
}
