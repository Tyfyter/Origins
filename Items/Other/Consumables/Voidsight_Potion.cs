using Origins.Buffs;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Voidsight_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Voidsight Potion");
			Tooltip.SetDefault("Significantly increases visibility");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Voidsight_Buff.ID;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Fireblossom);
			recipe.AddIngredient(ItemID.NightOwlPotion, 2);
			recipe.AddIngredient(ModContent.ItemType<Duskarp>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
