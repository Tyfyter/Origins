using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Bile_Flask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flask of Black Bile");
			Tooltip.SetDefault("Melee and Whip attacks stun enemies");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlaskofCursedFlames);
			Item.buffType = Weapon_Imbue_Bile.ID;
			Item.buffTime = 60 * 60 * 20;
			Item.value = Item.buyPrice(silver: 25);
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Black_Bile>(), 2);
			recipe.AddTile(TileID.ImbuingStation);
			recipe.Register();
		}
	}
}
