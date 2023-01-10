using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Fervor_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fervor Potion");
			Tooltip.SetDefault("Increases attack speed by 10%");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Fervor_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Prikish>());
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wilting_Rose_Item>());
			//recipe.AddRecipeGroup(OriginSystem.DeathweedRecipeGroupID);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
