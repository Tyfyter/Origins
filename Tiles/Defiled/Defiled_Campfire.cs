using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Defiled {
	public class Defiled_Campfire : CampfireBase {
		public override Vector3 Light => new(1.4f, 1.4f, 1.4f);
		public override Color MapColor => new(200, 200, 200);
	}
	public class Defiled_Campfire_Item : ModItem {
		public override void SetStaticDefaults() {
			ModCompatSets.AnyCampfires[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Defiled_Campfire>());
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(RecipeGroupID.Wood, 10)
			.AddIngredient<Defiled_Torch>(5)
			.Register();
		}
	}
}
