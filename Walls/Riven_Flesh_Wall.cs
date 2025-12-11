using Microsoft.Xna.Framework;
using Origins.Tiles;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Riven_Flesh_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.Conversion.Stone[Type] = true;
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			OriginsSets.Walls.RivenWalls[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(40, 140, 200));
			DustType = DustID.GemEmerald;
			DustType = Riven_Hive.DefaultTileDust;
		}
		public override void RandomUpdate(int i, int j) {
			Shelf_Coral shelfCoral = GetInstance<Shelf_Coral>();
			if (shelfCoral.CanGenerate(i, j, out double weight) && Math.Pow(weight, 4) > WorldGen.genRand.NextFloat() && TileExtenstions.CanActuallyPlace(i, j, shelfCoral.Type, 0, 0, out TileObject objectData, onlyCheck: false) && TileObject.Place(objectData)) {
				Point16 topLeft = TileObjectData.TopLeft(i, j);

				int id = GetInstance<Shelf_Coral_TE>().Place(topLeft.X, topLeft.Y);
				((Shelf_Coral_TE)TileEntity.ByID[id]).CurrentState = Shelf_Coral_TE.State.In;
			}
		}
	}
	public class Riven_Flesh_Wall_Safe : Riven_Flesh_Wall {
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
	}
}
