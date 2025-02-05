using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Sour_Apple : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(156, 240, 46),
				new Color(116, 170, 45),
				new Color(54, 118, 26)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed2,
				60 * 60 * 7
			);
		}
		/*public override void AddRecipes() {
			Recipe.Create(ItemID.FruitJuice))
			.AddIngredient(ItemID.Bottle)
			.AddIngredient(this, 2) we need to create a new recipe group including origins' fruits
			.AddTile(TileID.CookingPots)
			.Register();

			Recipe.Create(ItemID.FruitSalad))
			.AddIngredient(ItemID.Bowl)
			.AddIngredient(this, 3) we need to create a new recipe group including origins' fruits
			.AddTile(TileID.CookingPots)
			.Register();
		}*/
	}
}
