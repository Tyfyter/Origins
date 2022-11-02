using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Adaptability_Potion : ModItem {
		public override string Texture => "Terraria/Images/Buff_160";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Adaptability Potion");
			Tooltip.SetDefault("10% class stat share");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Adaptability_Buff.ID;
		}
		public override void AddRecipes() {
			//Recipe recipe = Recipe.Create(Type);
			//recipe.AddIngredient(ItemID.BottledWater);
			//recipe.AddIngredient(ModContent.ItemType<Prikish>());
			//recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wilting_Rose_Item>());
			//recipe.AddRecipeGroup(OriginSystem.DeathweedRecipeGroupID);
			//recipe.AddTile(TileID.Bottles);
			//recipe.Register();
		}
	}
}
