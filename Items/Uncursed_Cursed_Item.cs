using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items {
	public class Uncursed_Cursed_Item<TCursed> : ModItem where TCursed : ModItem {
		public override string Texture => typeof(TCursed).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<TCursed>());
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<TCursed>())
			.AddTile(TileID.BewitchingTable)
			.Register();
		}
	}
}
