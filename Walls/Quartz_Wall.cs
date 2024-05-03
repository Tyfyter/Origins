using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Quartz_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn[Type] = true;
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
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Quartz_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Quartz_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
