using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Quantum_Injector : ModItem {
		public const int mana_per_use = 10;
		public const int max_uses = 20;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Quantum Injector");
			Tooltip.SetDefault("Permanently increases maximum mana by 10");
			SacrificeTotal = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ButterscotchRarity.ID;
			Item.UseSound = SoundID.Item90;
		}
		public override bool? UseItem(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.quantumInjectors < max_uses) {
				originPlayer.quantumInjectors++;
				return true;
			}
			return false;
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
