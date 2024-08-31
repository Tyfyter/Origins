using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Bileberry : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(81, 52, 161),
				new Color(22, 18, 33)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 5
			);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Shattered_Stout>());
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddIngredient(this);
			recipe.AddIngredient(ModContent.ItemType<Prickly_Pear>());
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
			
			/*recipe = Recipe.Create(ItemID.FruitJuice));
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddIngredient(this, 2); we need to create a new recipe group including origins' fruits
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.FruitSalad));
			recipe.AddIngredient(ItemID.Bowl);
			recipe.AddIngredient(this, 3); we need to create a new recipe group including origins' fruits
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();*/
		}
	}
}
