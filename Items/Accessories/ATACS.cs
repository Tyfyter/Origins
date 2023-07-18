using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class ATACS : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("ATACS");
			// Tooltip.SetDefault("10% increased critical strike chance\nDisplays the trajectory of projectiles from your selected item");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 10f;
			//player.GetModPlayer<OriginPlayer>().strangeComputer = true; red laser
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.EyeoftheGolem);
			recipe.AddIngredient(ModContent.ItemType<Strange_Computer>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
