using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Tainted_Stone_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.Conversion.Stone[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			Origins.WallHammerRequirement[Type] = 70;
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(73, 42, 22));
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
	public class Tainted_Stone_Wall_Safe : Tainted_Stone_Wall {
		public override string Texture => "Origins/Walls/Tainted_Stone_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
			Origins.WallHammerRequirement[Type] = 0;
		}
	}
	public class Tainted_Stone_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Tainted_Stone_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(OriginTile.TileItem<Tainted_Stone>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(OriginTile.TileItem<Tainted_Stone>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
