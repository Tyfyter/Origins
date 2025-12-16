using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Ashen_Campfire : CampfireBase {
		public override Vector3 Light => new(1.4f, 0.7f, 0.15f);
		public override Color MapColor => new(191, 81, 50);
	}
	public class Ashen_Campfire_Item : ModItem {
		public override void SetStaticDefaults() {
			ModCompatSets.AnyCampfires[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Ashen_Campfire>());
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(RecipeGroupID.Wood, 10)
			.AddIngredient<Ashen_Torch>(5)
			.Register();
		}
	}
}
