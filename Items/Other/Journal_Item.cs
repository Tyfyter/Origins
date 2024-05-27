using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other {
	public class Journal_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FallenStar);
			Item.ammo = AmmoID.None;
			Item.maxStack = 1;
		}
		public override bool? UseItem(Player player) {
			ref bool isUnlocked = ref Main.LocalPlayer.GetModPlayer<OriginPlayer>().journalUnlocked;
			if (!isUnlocked) {
				isUnlocked = true;
				Item.TurnToAir();
			}
			return true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.Feather);
			recipe.AddIngredient(ItemID.BlackInk);
			recipe.AddOnCraftCallback((_, i, _, _) => {
				ref bool isUnlocked = ref Main.LocalPlayer.GetModPlayer<OriginPlayer>().journalUnlocked;
				if (!isUnlocked) {
					isUnlocked = true;
					i.TurnToAir();
				}
			});
			recipe.Register();
		}
	}
}
