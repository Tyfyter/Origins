using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Control_Locus : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.RangedBoostAcc,
			WikiCategories.ExplosiveBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(14, 28);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Orange;
			Item.master = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Stack_of_Shraps>())
			.AddIngredient(ModContent.ItemType<Weakpoint_Analyzer>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().controlLocus = true;
		}
	}
}
