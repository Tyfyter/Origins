using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using AltLibrary.Common.Systems;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using ReLogic.Content;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Origins.Dev {
	public class RecipeGroupPage : WikiSpecialPage {
		public override string Name => "Recipe_Groups";
		public override void ModifyContext(Dictionary<string, object> context) {
			Dictionary<int, List<Recipe>> recipesByUsedGroups = [];
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				if (recipe.Mod is not Origins) continue;
				for (int j = 0; j < recipe.acceptedGroups.Count; j++) {
					if (!recipesByUsedGroups.ContainsKey(recipe.acceptedGroups[j])) recipesByUsedGroups.Add(recipe.acceptedGroups[j], []);
					recipesByUsedGroups[recipe.acceptedGroups[j]].Add(recipe);
				}
			}
			if (recipesByUsedGroups.TryGetValue(RecipeGroups.ShadowScales.RegisteredId, out List<Recipe> recipes)) context["EvilBossMaterialRecipes"] = recipes;
			if (recipesByUsedGroups.TryGetValue(RecipeGroups.Deathweed.RegisteredId, out recipes)) context["EvilHerbRecipes"] = recipes;
			if (recipesByUsedGroups.TryGetValue(RecipeGroups.CursedFlames.RegisteredId, out recipes)) context["EvilSubstanceRecipes"] = recipes;
			if (recipesByUsedGroups.TryGetValue(RecipeGroups.RottenChunks.RegisteredId, out recipes)) context["EvilTissueRecipes"] = recipes;
			if (recipesByUsedGroups.TryGetValue(OriginSystem.GemStaffRecipeGroupID, out recipes)) context["GemStaffRecipes"] = recipes;
			if (recipesByUsedGroups.TryGetValue(OriginSystem.EvilBoomerangRecipeGroupID, out recipes)) context["EvilBoomerangRecipes"] = recipes;
		}
		public override IEnumerable<(string name, (Texture2D texture, int frames)[] texture)> GetAnimatedSprites() {
			HashSet<RecipeGroup> usedGroups = [];
			for (int i = 0; i < Main.recipe.Length && usedGroups.Count < RecipeGroup.recipeGroups.Count; i++) {
				Recipe recipe = Main.recipe[i];
				if (recipe.Mod is not Origins) continue;
				for (int j = 0; j < recipe.acceptedGroups.Count; j++) {
					usedGroups.Add(RecipeGroup.recipeGroups[recipe.acceptedGroups[j]]);
				}
			}
			foreach (RecipeGroup group in RecipeGroup.recipeGroups.Values) {
				if (usedGroups.Contains(group) || group.ValidItems.Any(type => ContentSamples.ItemsByType[type].ModItem?.Mod is Origins)) yield return ("RecipeGroups/" + GetRecipeGroupWikiName(group.RegisteredId), GetTexuresForGroup(group).ToArray());
			}
		}
		public static IEnumerable<(Texture2D texture, int frames)> GetTexuresForGroup(RecipeGroup group) {
			string name = GetRecipeGroupWikiName(group.RegisteredId);
			Vector2 maxSize = default;
			foreach (int type in group.ValidItems) {
				if (TextureAssets.Item[type].State == AssetState.NotLoaded) {
					Main.Assets.Request<Texture2D>(TextureAssets.Item[type].Name, AssetRequestMode.ImmediateLoad);
				}
				maxSize = Vector2.Max(maxSize, TextureAssets.Item[type].Size());
			}
			foreach(int type in group.ValidItems) {
				yield return (SpriteGenerator.Generate(spriteBatch => {
					spriteBatch.Draw(TextureAssets.Item[type].Value, maxSize * 0.5f, null, Color.White, 0, TextureAssets.Item[type].Size() * 0.5f, 1, SpriteEffects.None, 0f);
				}, ((int)maxSize.X, (int)maxSize.Y)), 60);
			}
		}
		public static string GetRecipeGroupWikiName(int recipeGroup) => Lang.GetItemNameValue(RecipeGroup.recipeGroups[recipeGroup].IconicItemId).Replace(" ", "");
	}
}
