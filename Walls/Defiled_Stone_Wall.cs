using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;

namespace Origins.Walls {
	public class Defiled_Stone_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe;
		public override Color MapColor => new(150, 150, 150);
		public override int DustType => Defiled_Wastelands.DefaultTileDust;
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => OriginTile.TileItem<Defiled_Stone>();
		public override string Name => WallVersion == WallVersion.Natural ? base.Name.Replace("_Natural", "") : base.Name;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Stone[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			if (WallVersion == WallVersion.Natural) Origins.WallHammerRequirement[Type] = 70;
		}
		public override void Load() {/*
			if (WallVersion == WallVersion.Safe) {
				Chambersite_Ore_Wall.Create(this, Item, () => DustType, itemOverlay: Chambersite_Ore_Wall.overlay_path_base + "Item_Chunk", legacyNames: "Chambersite_Defiled_Stone_Wall");
			}*/
		}
	}
/*	public class Defiled_Stone_Wall_Safe : Defiled_Stone_Wall {
		public override string Texture => "Origins/Walls/Defiled_Stone_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
			Origins.WallHammerRequirement[Type] = 0;
		}
	}
	public class Defiled_Stone_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Defiled_Stone_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(OriginTile.TileItem<Defiled_Stone>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(OriginTile.TileItem<Defiled_Stone>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}*/
}
