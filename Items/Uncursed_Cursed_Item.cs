using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items {
	public class Uncursed_Cursed_Item<TCursed> : ModItem where TCursed : ModItem {
		public virtual bool HasOwnTexture => false;
		public override string Texture => HasOwnTexture ? base.Texture : typeof(TCursed).GetDefaultTMLName();
		public override void SetDefaults() {
			ItemSlotSet slots = new(Item);
			Item.CloneDefaults(ModContent.ItemType<TCursed>());
			if (HasOwnTexture) slots.Apply(Item);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<TCursed>())
			.AddTile(TileID.BewitchingTable)
			.Register();
		}
		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
			return equippedItem.ModItem is not TCursed;
		}
	}
}
