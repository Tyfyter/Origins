using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Iron_Lung : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 36);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			oP.pacemaker = true;
			oP.ironLung = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Air_Tank>()
			.AddIngredient<Pacemaker>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
