﻿using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Absorption_Potion : ModItem {
        public string[] Categories => [
            "Potion",
			"SelfDamageProtek"
		];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Safe_Buff.ID;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.BombFish)
			.AddIngredient(ItemID.FallenStar)
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
}
