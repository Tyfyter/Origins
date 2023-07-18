using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other {
	public class Journal_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Journal");
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.Feather);
			recipe.AddIngredient(ItemID.BlackInk);
			recipe.AddCondition(NetworkText.Empty, (_) => !Main.LocalPlayer.GetModPlayer<OriginPlayer>().journalUnlocked);
			recipe.AddOnCraftCallback((_, i, _) => {
				Main.LocalPlayer.GetModPlayer<OriginPlayer>().journalUnlocked = true;
				i.TurnToAir();
			});
			recipe.Register();
		}
	}
}
