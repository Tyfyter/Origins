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

namespace Origins.Tiles.Riven {
	public class Riven_Campfire : CampfireBase {
		public override Vector3 Light => new Vector3(0.3f, 2.10f, 1.90f) * Riven_Hive.NormalGlowValue.GetValue();
		public override Color MapColor => new Color(217, 95, 54);
	}
	public class Riven_Campfire_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Riven_Campfire>());
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(RecipeGroupID.Wood, 10)
			.AddIngredient<Riven_Torch>(5)
			.Register();
		}
	}
}
