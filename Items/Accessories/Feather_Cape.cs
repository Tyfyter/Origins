using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Feather_Cape : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1, silver: 85);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Feathery_Crest>()
			.AddIngredient<Superjump_Cape>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.jumpSpeedBoost += 12;
			player.noFallDmg = true;
			if (player.controlJump) {
				player.gravity = 0.15f;
			} else if (player.controlDown && player.velocity.Y != 0f) {
				player.gravity = 1.4f;
			}
		}
	}
}
