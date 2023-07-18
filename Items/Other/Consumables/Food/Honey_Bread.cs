using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Honey_Bread : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Honey Wheat Bread");
			// Tooltip.SetDefault("{$CommonItemTooltip.MajorStats}\n'Let's get this bread!'");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = BuffID.WellFed3;
			Item.buffTime = 60 * 60 * 15;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.White;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Honey, Item.buffTime);
			return true;
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
