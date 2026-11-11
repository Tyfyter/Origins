using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class Fiberwrap : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 36);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().medicalMask = true;
			player.blackBelt = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BlackBelt)
			.AddIngredient<Medical_Mask>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
