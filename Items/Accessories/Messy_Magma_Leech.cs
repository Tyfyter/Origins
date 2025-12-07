using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Messy_Magma_Leech : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 26);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().magmaLeech = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.MagmaStone)
			.AddIngredient(ModContent.ItemType<Messy_Leech>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
