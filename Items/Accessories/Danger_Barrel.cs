using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Danger_Barrel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(gold: 2, silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().dangerBarrel = true;
			player.GetModPlayer<OriginPlayer>().explosiveFuseTime *= 0.667f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.MagmaStone)
			.AddIngredient<IWTPA_Standard>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}