using Origins.Dev;
using Origins.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Artifiber : OriginTile {
		public string[] Categories => [
			WikiCategories.Plant
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(143, 114, 94));
			mergeID = TileID.WoodBlock;
			DustType = DustID.WoodFurniture;
		}
	}
	public class Artifiber_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Artifiber>());
		}
		public override void AddRecipes() {
			CreateRecipe(ModContent.ItemType<Artifiber_Wall_Item>(), 4)
			.AddRecipeGroup(RecipeGroupID.Wood, 10)
			.AddIngredient<Ashen_Torch>(5)
			.Register();
		}
	}
}
