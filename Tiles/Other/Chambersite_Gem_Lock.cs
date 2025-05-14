using Origins.Items.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Chambersite_Gem_Lock : ModGemLock {
		public override int GemType => ModContent.ItemType<Large_Chambersite>();
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Chambersite_Item>(5)
				.AddIngredient(ItemID.StoneBlock, 10)
				.Register();
			};
		}
	}
}