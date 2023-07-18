using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Focus_Potion : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Focus Potion");
			// Tooltip.SetDefault("Mana is used to increase damage");
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Focus_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ModContent.ItemType<Brineglow>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
