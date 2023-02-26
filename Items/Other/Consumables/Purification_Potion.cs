using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Purification_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Purification Potion");
			Tooltip.SetDefault("Grants immunity to being assimilated");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Purification_Buff.ID;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.FallenStar, 4);
			recipe.AddIngredient(ItemID.PurificationPowder, 10);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
