using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Salt_Flask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flask of Alkahest");
			Tooltip.SetDefault("Melee and Whip attacks tenderize targets");//for some reason Whip is capitalized in the vanilla ones
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlaskofIchor);
			Item.buffType = Weapon_Imbue_Salt.ID;
			Item.buffTime = 60 * 60 * 20;
			Item.value = Item.sellPrice(silver: 5);
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Alkahest>(), 2);
			recipe.AddTile(TileID.ImbuingStation);
			recipe.Register();
		}
	}
}
