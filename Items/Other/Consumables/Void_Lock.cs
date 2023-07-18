using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Void_Lock : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Void Lock");
			// Tooltip.SetDefault("Can be used to lock some chests only unlockable by you");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Bone, 12);
			recipe.AddIngredient(ItemID.Chain, 2);
			recipe.AddIngredient(ItemID.JungleSpores, 5);
			recipe.AddRecipeGroupWithItem(OriginSystem.ShadowScaleRecipeGroupID, showItem: ItemID.ShadowScale, 10);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();

			/*recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Bone, 7);
			recipe.AddIngredient(ItemID.ChestLock);
			recipe.AddIngredient(ItemID.JungleSpores, 5);
			recipe.AddRecipeGroupWithItem(OriginSystem.ShadowScaleRecipeGroupID, showItem: ItemID.ShadowScale, 10);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();*/
		}
	}
}