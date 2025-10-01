using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Tainted_Stone_Wall : ModWall {
		public override string Texture => typeof(Defiled_Stone_Wall).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			WallID.Sets.Conversion.Stone[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			Origins.WallHammerRequirement[Type] = 70;
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(150, 150, 150));
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
	public class Tainted_Stone_Wall_Safe : Tainted_Stone_Wall {
		public override string Texture => "Origins/Walls/Defiled_Stone_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
			Origins.WallHammerRequirement[Type] = 0;
		}
	}
	public class Tainted_Stone_Wall_Item : ModItem {
		public override string Texture => typeof(Defiled_Stone_Wall_Item).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Tainted_Stone_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Tainted_Stone_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Tainted_Stone_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
