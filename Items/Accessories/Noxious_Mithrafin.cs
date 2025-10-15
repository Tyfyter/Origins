using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Noxious_Mithrafin : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Mithrafin)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Venom_Fang)}.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.mithrafin = true;
			originPlayer.venomFang = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Mithrafin>()
			.AddIngredient<Venom_Fang>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
