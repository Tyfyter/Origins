using Origins.Tiles.Other;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Lost_Brick_Wall : ModWall {
		public override void SetStaticDefaults() {
			AddMapEntry(new Color(151, 151, 151));
			DustType = Defiled_Wastelands.DefaultTileDust;
			Main.wallHouse[Type] = true;
		}
	}
	public class Lost_Brick_Wall_Item : ModItem {
		public override string Texture => "Origins/Tiles/Other/Lost_Brick_Item";
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Lost_Brick_Wall>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Lost_Brick_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Lost_Brick_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
