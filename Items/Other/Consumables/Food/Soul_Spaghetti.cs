using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Origins.Items.Other.Consumables.Food {
	public class Soul_Spaghetti : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(129, 129, 129),
				new Color(88, 88, 88),
				new Color(66, 66, 66)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed2,
				60 * 60 * 8
			);
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BowlofSoup)
			.AddIngredient(ModContent.ItemType<Strange_String>(), 10)
			.AddTile(TileID.CookingPots)
			.Register();
		}
	}
}
