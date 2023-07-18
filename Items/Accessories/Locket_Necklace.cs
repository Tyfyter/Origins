using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Locket_Necklace : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Locket Necklace");
			// Tooltip.SetDefault("Increases damage and movement speed after taking damage");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 32);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.PanicNecklace);
			recipe.AddIngredient(ModContent.ItemType<Comb>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().guardedHeart = true;
		}
	}
}
