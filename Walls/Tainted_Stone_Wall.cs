using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Walls {
    public class Tainted_Stone_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe;
		public override Color MapColor => new(73, 42, 22);
		public override int DustType => Ashen_Biome.DefaultTileDust;
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => OriginTile.TileItem<Tainted_Stone>();
		public override string Name => WallVersion == WallVersion.Natural ? base.Name.Replace("_Natural", "") : base.Name;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Stone[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			if (WallVersion == WallVersion.Natural) Origins.WallHammerRequirement[Type] = 70;
		}
		public override void Load() {
			if (WallVersion == WallVersion.Natural) {
				Mod.AddContent(new Auto_Chambersite_Wall(this, new(73, 42, 22), ModContent.GetInstance<Ashen_Alt_Biome>));
			}
		}
	}
/*	public class Tainted_Stone_Wall_Safe : Tainted_Stone_Wall {
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
	}*/
}
