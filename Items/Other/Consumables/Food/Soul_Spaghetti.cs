using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Items.Materials;

namespace Origins.Items.Other.Consumables.Food {
	public class Soul_Spaghetti : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Soul Spaghetti");
			// Tooltip.SetDefault("{$CommonItemTooltip.MediumStats}\n'Tastes like noodle with hairs and meats'");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = BuffID.WellFed2;
			Item.buffTime = 60 * 60 * 8;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BowlofSoup);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 10);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}
