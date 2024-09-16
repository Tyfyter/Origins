using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Honey_Wheat_Bread : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(216, 209, 135),
				new Color(209, 188, 92),
				new Color(181, 148, 58)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed3,
				60 * 60 * 4
			);
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Honey, Item.buffTime);
			return true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Honey_Wheat_Bread>());
			recipe.AddIngredient(ItemID.BottledHoney);
			recipe.AddIngredient(ModContent.ItemType<Bread>());
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}
