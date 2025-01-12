using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class ACME_Crate : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.4f;
			player.GetModPlayer<OriginPlayer>().magicTripwire = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Magic_Tripwire>()
			.AddIngredient<Nitro_Crate>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
