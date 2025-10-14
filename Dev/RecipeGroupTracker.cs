using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Origins.Items.Other.Consumables.Medicine;

namespace Origins.Dev {
	internal class RecipeGroupTracker : ModSystem {
		public override void PostSetupRecipes() {
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				for (int j = 0; j < recipe.acceptedGroups.Count; j++) {
					recipe.requiredItem
					.Find(item => RecipeGroup.recipeGroups[recipe.acceptedGroups[j]]
					.ContainsItem(item.type))
					.GetGlobalItem<RecipeGroupTrackerGlobalItem>()
					.recipeGroup = recipe.acceptedGroups[j];
				}
			}
		}
	}
	internal class RecipeGroupTrackerGlobalItem : GlobalItem {
		public override bool InstancePerEntity => true;
		public int recipeGroup = -1;
	}
}
