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
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 5
			);
		}
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Shattered_Stout>())
			.AddIngredient(ItemID.Bottle)
			.AddIngredient(this)
			.AddIngredient(ModContent.ItemType<Prickly_Pear>())
			.AddTile(TileID.CookingPots)
			.Register();
			
			/*Recipe.Create(ItemID.FruitJuice))
			.AddIngredient(ItemID.Bottle)
			.AddIngredient(this, 2) we need to create a new recipe group including origins' fruits
			.AddTile(TileID.CookingPots)
			.Register();

			Recipe.Create(ItemID.FruitSalad))
			.AddIngredient(ItemID.Bowl)
			.AddIngredient(this, 3) we need to create a new recipe group including origins' fruits
			.AddTile(TileID.CookingPots)
			.Register();*/
		}
	}
}
