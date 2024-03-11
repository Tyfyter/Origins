using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Dev {
	public class RecipePage : WikiSpecialPage {
		public override void ModifyContext(Dictionary<string, object> context) {
			(List<Recipe> recipes, List<Recipe> usedIn) = WikiExtensions.GetRecipes(WikiExtensions.GetRecipeAllItemCondition(item => item?.ModItem is ModItem modItem && modItem.Mod is Origins));
			recipes.AddRange(usedIn);
			context["Recipes"] = recipes;
		}
	}
}
