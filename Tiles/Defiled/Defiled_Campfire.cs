using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Campfire : CampfireBase {
		public override Vector3 Light => new Vector3(1.4f, 1.4f, 1.4f);
		public override Color MapColor => new Color(200, 200, 200);
	}
	public class Defiled_Campfire_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Defiled_Campfire>());
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(RecipeGroupID.Wood, 10)
			.AddIngredient<Defiled_Torch>(5)
			.Register();
		}
	}
}
