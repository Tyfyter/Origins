using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Absorption_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Absorption Potion");
			Tooltip.SetDefault("Full protection from explosive self-damage");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Safe_Buff.ID;
			Item.buffTime = 60 * 60 * 8;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.BombFish);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
