using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Antisolve_Potion : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Antisolve_Buff.ID;
			Item.buffTime = 60 * 60 * 8;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
			recipe.AddIngredient(ModContent.ItemType<Brineglow_Item>());
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss_Item>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
