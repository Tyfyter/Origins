using Origins.Tiles;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Riven_Flesh_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe;
		public override Color MapColor => new(40, 140, 200);
		public override int DustType => Riven_Hive.DefaultTileDust;
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => OriginTile.TileItem<Spug_Flesh>();
		public override string Name => WallVersion == WallVersion.Natural ? base.Name.Replace("_Natural", "") : base.Name;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Stone[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			if (WallVersion == WallVersion.Natural) {
				OriginsSets.Walls.RivenWalls[Type] = true;
				Origins.WallHammerRequirement[Type] = 70;
			}
		}
		public override void Load() {/*
			if (WallVersion == WallVersion.Safe) {
				Chambersite_Ore_Wall.Create(this, Item, () => DustType, itemOverlay: Chambersite_Ore_Wall.overlay_path_base + "Item_Flesh", legacyNames: "Chambersite_Riven_Flesh_Wall");
			}*/
		}
		public override void RandomUpdate(int i, int j) {
			if (WallVersion == WallVersion.Natural) {
				Shelf_Coral shelfCoral = GetInstance<Shelf_Coral>();
				if (shelfCoral.CanGenerate(i, j, out double weight) && Math.Pow(weight, 4) > WorldGen.genRand.NextFloat() && TileExtenstions.CanActuallyPlace(i, j, shelfCoral.Type, 0, 0, out TileObject objectData, onlyCheck: false) && TileObject.Place(objectData)) {
					Point16 topLeft = TileObjectData.TopLeft(i, j);

					int id = GetInstance<Shelf_Coral_TE>().Place(topLeft.X, topLeft.Y);
					((Shelf_Coral_TE)TileEntity.ByID[id]).CurrentState = Shelf_Coral_TE.State.In;
				}
			}
		}
	}
/*	public class Riven_Flesh_Wall_Safe : Riven_Flesh_Wall {
		public override string Texture => "Origins/Walls/Riven_Flesh_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
			OriginsSets.Walls.RivenWalls[Type] = false;
		}
		public override void RandomUpdate(int i, int j) { }
	}
	public class Riven_Flesh_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Riven_Flesh_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(OriginTile.TileItem<Spug_Flesh>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(OriginTile.TileItem<Spug_Flesh>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}*/
}
