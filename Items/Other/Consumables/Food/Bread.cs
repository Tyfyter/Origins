using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Bread : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bread");
			// Tooltip.SetDefault("{$CommonItemTooltip.MinorStats}\n'Let's get this bread!'");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 8;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Honey_Bread>());
			recipe.AddIngredient(ItemID.BottledHoney);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}
