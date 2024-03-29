﻿using Origins.Buffs;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Tiles.Ashen;

namespace Origins.Items.Other.Consumables {
    public class Ambition_Potion : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Ambition_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Polyeel>());
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Surveysprout_Item>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
