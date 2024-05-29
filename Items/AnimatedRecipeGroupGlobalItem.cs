using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Items {
	internal class AnimatedRecipeGroupGlobalItem : GlobalItem {
		public override bool InstancePerEntity => true;
		int recipeGroup = -1;
		public static void PostSetupRecipes() {
			for (int i = 0; i < Main.recipe.Length; i++) {
				Recipe recipe = Main.recipe[i];
				for (int j = 0; j < recipe.acceptedGroups.Count; j++) {
					recipe.requiredItem
					.Find(item => RecipeGroup.recipeGroups[recipe.acceptedGroups[j]]
					.ContainsItem(item.type))
					.GetGlobalItem<AnimatedRecipeGroupGlobalItem>()
					.recipeGroup = recipe.acceptedGroups[j];
				}
			}
		}
		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (recipeGroup != -1) {
				RecipeGroup group = RecipeGroup.recipeGroups[recipeGroup];
				int itemType = group.ValidItems.Skip((int)(Main.timeForVisualEffects / 60) % group.ValidItems.Count).First();
				Main.instance.LoadItem(itemType);
				Texture2D texture = TextureAssets.Item[itemType].Value;
				frame = (Main.itemAnimations[itemType] is DrawAnimation animation) ? animation.GetFrame(texture) : texture.Frame();
				origin = frame.Size() * 0.5f;
				spriteBatch.Draw(
					texture,
					position,
					frame,
					drawColor,
					0f,
					origin,
					scale,
					SpriteEffects.None,
				0f);
				return false;
			}
			return true;
		}
	}
}
