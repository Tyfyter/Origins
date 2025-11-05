using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Soot_Sandstone_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn[Type] = true;
			WallID.Sets.Conversion.Sandstone[Type] = true;
			Main.wallBlend[Type] = WallID.Sandstone;//what wall type this wall is considered to be when blending
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(63, 55, 77));
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
	public class Soot_Sandstone_Wall_Safe : Soot_Sandstone_Wall {
		public override string Texture => "Origins/Walls/Soot_Sandstone_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Soot_Sandstone_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Soot_Sandstone_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Soot_Sandstone_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Soot_Sandstone_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
