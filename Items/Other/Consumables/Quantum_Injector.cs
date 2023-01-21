using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Quantum_Injector : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Quantum Injector");
			Tooltip.SetDefault("Permanently increases maximum mana by 10");
			SacrificeTotal = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ButterscotchRarity.ID;
			Item.manaIncrease += 10; //Max of 20
			Item.UseSound = SoundID.Item90;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ManaCrystal);
			recipe.AddIngredient(ModContent.ItemType<Qube>(), 20);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
