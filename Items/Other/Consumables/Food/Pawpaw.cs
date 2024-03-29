﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Pawpaw : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.BloodOrange);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 5;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Jelly_Schnapps>());
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddIngredient(this);
			recipe.AddIngredient(ModContent.ItemType<Periven>());
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
