using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Brittle_Quartz_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn[Type] = true;
			WallID.Sets.Conversion.HardenedSand[Type] = true;
			Main.wallBlend[Type] = WallID.HardenedSand;//what wall type this wall is considered to be when blending
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(115, 115, 115));
			DustType = DustID.Ghost;
		}
	}
	public class Brittle_Quartz_Wall_Safe : Brittle_Quartz_Wall {
		public override string Texture => "Origins/Walls/Brittle_Quartz_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Brittle_Quartz_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Brittle_Quartz_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Brittle_Quartz_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Brittle_Quartz_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
