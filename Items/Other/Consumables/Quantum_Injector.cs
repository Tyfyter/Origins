using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Quantum_Injector : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Quantum Injector");
			Tooltip.SetDefault("Restores 400 mana");
			SacrificeTotal = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaPotion);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ButterscotchRarity.ID;
			Item.healMana = 400;
			Item.UseSound = SoundID.Item90;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SuperManaPotion);
			recipe.AddIngredient(ModContent.ItemType<Quantium>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
