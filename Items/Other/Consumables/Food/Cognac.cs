using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Cognac : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0xFF5100)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 20,
				true
			);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ItemID.Bottle)
			.AddIngredient<Blastberry>()
			.AddIngredient<Tangerine>()
			.AddTile(TileID.CookingPots)
			.Register();
	}
}
