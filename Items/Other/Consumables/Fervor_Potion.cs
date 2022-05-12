﻿using Origins.Buffs;
using Origins.Items.Other.Fish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Fervor_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fervor Potion");
			Tooltip.SetDefault("Increases attack speed by 10%");
		}
		public override void SetDefaults() {
			item.CloneDefaults(ItemID.WrathPotion);
			item.buffType = Fervor_Buff.ID;
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Prikish>());
			recipe.AddIngredient(ItemID.Deathweed);
			recipe.AddTile(TileID.Bottles);
			recipe.alchemy = true;
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}