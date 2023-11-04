using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Makeover_Choker : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 18);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 10);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.longInvince = true;
			Mysterious_Spray.EquippedEffect(player);
			if (!hideVisual) {
				Mysterious_Spray.VanityEffect(player);
			}
		}
		public override void UpdateVanity(Player player) {
			Mysterious_Spray.VanityEffect(player);
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.CrossNecklace);
			recipe.AddIngredient(ModContent.ItemType<Mysterious_Spray>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
