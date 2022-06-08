using Origins.Buffs;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Fervor_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fervor Potion");
			Tooltip.SetDefault("Increases attack speed by 10%");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Fervor_Buff.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ModContent.ItemType<Prikish>());
			recipe.AddIngredient(ItemID.Deathweed);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
