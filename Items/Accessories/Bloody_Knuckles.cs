using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Bloody_Knuckles : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(34, 34);
			Item.defense = 8;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().medicalMask = true;
			player.blackBelt = true;
			player.aggro += 400;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.FleshKnuckles)
			.AddIngredient<Medical_Mask>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
