using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
    public class CFHES : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(gold: 4, silver: 20);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.6f;
			player.GetModPlayer<OriginPlayer>().magicTripwire = true;

			player.GetModPlayer<OriginPlayer>().dangerBarrel = true;
			player.GetModPlayer<OriginPlayer>().explosiveFuseTime *= 0.666f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<ACME_Crate>()
			.AddIngredient<Danger_Barrel>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}