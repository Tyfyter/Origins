using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Fiberglass_Wall : ModWall {
		public override void SetStaticDefaults() {
			AddMapEntry(new Color(16, 83, 122));
			HitSound = SoundID.Shatter;
			DustType = DustID.Glass;
		}
	}
	public class Fiberglass_Wall_Safe : Fiberglass_Wall {
		public override string Texture => "Origins/Walls/Fiberglass_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Fiberglass_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GlassWall);
			Item.createWall = WallType<Fiberglass_Wall_Safe>();
		}
	}
}
