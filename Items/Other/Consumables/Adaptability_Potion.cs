using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Adaptability_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Protean Potion");
			Tooltip.SetDefault("Half of any weapon boosts are shared across all classes");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Adaptability_Buff.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Bonehead_Jellyfish>());
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wrycoral_Item>(), 20);
			recipe.AddRecipeGroup(OriginSystem.DeathweedRecipeGroupID);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
