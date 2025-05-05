using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Encrusted_Brick_Wall : ModWall {
		public override void SetStaticDefaults() {
			AddMapEntry(new Color(70, 110, 177));
			DustType = DustID.Astra;
			Main.wallHouse[Type] = true;
		}
	}
	public class Encrusted_Brick_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Encrusted_Brick_Wall>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Encrusted_Brick_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Encrusted_Brick_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
