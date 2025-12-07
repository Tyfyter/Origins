using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Makeover_Choker : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 18);
			Item.rare = ItemRarityID.LightRed;
			Item.master = true;
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
			CreateRecipe()
			.AddIngredient(ItemID.CrossNecklace)
			.AddIngredient(ModContent.ItemType<Mysterious_Spray>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
