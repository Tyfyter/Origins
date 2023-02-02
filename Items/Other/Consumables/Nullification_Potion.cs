using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Nullification_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nullification Potion");
			Tooltip.SetDefault("Removes all current harmful effects");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffTime = 60;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.StrangeBrew, 5);
			recipe.AddIngredient(ModContent.ItemType<Lunar_Token>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
		public override bool? UseItem(Player player) {
			//debuff removal
			for (int i = 0; i < BuffLoader.BuffCount; i++) {
				if (Main.debuff[i] && !BuffID.Sets.NurseCannotRemoveDebuff[i]) {
					player.buffImmune[i] = true;
				}
			}
			player.buffImmune[BuffID.Suffocation] = true; // 
			return null;
		}
	}
}
