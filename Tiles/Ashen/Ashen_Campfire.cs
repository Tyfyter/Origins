using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Ashen_Campfire : CampfireBase {
		public override string Texture => typeof(Defiled_Campfire).GetDefaultTMLName();
		public override Vector3 Light => new Vector3(1.4f, 1.4f, 1.4f);
		public override Color MapColor => new Color(200, 200, 200);
	}
	public class Ashen_Campfire_Item : ModItem {
		public override string Texture => typeof(Defiled_Campfire_Item).GetDefaultTMLName();
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
