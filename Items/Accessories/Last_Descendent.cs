using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Last_Descendent : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Last Descendant");
			// Tooltip.SetDefault("Increases damage, movement speed, and length of invincibility after taking damage\nAdditionally causes stars to fall after taking damage");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.StarVeil);
			recipe.AddIngredient(ModContent.ItemType<Locket_Necklace>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			player.longInvince = true;
			player.starCloakItem = Item;
			player.starCloakItem_starVeilOverrideItem = Item;
			player.GetModPlayer<OriginPlayer>().guardedHeart = true;
		}
	}
}
