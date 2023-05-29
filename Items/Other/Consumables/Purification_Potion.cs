using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Purification_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Purification Potion");
			Tooltip.SetDefault("Grants immunity to being assimilated by world evils");
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
			recipe.AddIngredient(ItemID.Ectoplasm);
			recipe.AddIngredient(ItemID.Moonglow);
			recipe.AddIngredient(ModContent.ItemType<Mojo_Flask>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
