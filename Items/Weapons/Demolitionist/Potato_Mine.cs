using Origins.Items.Other.Consumables.Food;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Potato_Mine : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LandMine);
			Item.damage = 85;
			Item.createTile = ModContent.TileType<Potato_Mine_Tile>();
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 15);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ModContent.ItemType<Potato>());
			recipe.Register();
		}
	}
}
