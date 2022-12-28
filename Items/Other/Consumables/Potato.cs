using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Potato : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Potato");
			Tooltip.SetDefault("{$CommonItemTooltip.MinorStats}\nIt's a potato...");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ChocolateChipCookie);
			Item.holdStyle = ItemHoldStyleID.HoldUp;
			Item.scale = 0.75f;
			Item.createTile = ModContent.TileType<Potato_Tile>();
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 10;
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes() {
			//Recipe recipe = Recipe.Create(ModContent.ItemType<Hot_Potato>());
			//recipe.AddIngredient(ItemID.HellstoneBar, 14);
			//recipe.AddIngredient(this);
			//recipe.AddTile(TileID.DemonAltar);
			//recipe.Register();

			//recipe = Recipe.Create(ModContent.ItemType<Potato_Mine>(), 15);
			//recipe.AddIngredient(ItemID.ExplosivePowder);
			//recipe.AddIngredient(this);
			//recipe.AddTile(TileID.WorkBenches);
			//recipe.Register();
		}
	}
}
