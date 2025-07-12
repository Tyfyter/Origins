using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
	public class Riven_Campfire : CampfireBase {
		public override Vector3 Light => new Vector3(0.3f, 2.10f, 1.90f) * Riven_Hive.NormalGlowValue.GetValue();
		public override Color MapColor => new Color(20, 136, 182);
	}
	public class Riven_Campfire_Item : ModItem {
		public override void SetStaticDefaults() {
			ModCompatSets.AnyCampfires[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Riven_Campfire>());
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(RecipeGroupID.Wood, 10)
			.AddIngredient<Riven_Torch>(5)
			.Register();
		}
	}
}
