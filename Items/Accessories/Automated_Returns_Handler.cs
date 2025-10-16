using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Automated_Returns_Handler : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Movement,
			WikiCategories.RangedBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.hasMagiluminescence = true;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.turboReel2 = true;
			originPlayer.automatedReturnsHandler = true;

			player.blackBelt = true;
			player.dashType = 1;
			player.spikedBoots += 2;

			DelegateMethods.v3_1 = new Vector3(0.9f, 0.8f, 0.5f);
			Utils.PlotTileLine(player.Center, player.Center + player.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
			Utils.PlotTileLine(player.Left, player.Right, 20f, DelegateMethods.CastLightOpen);
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ItemID.MasterNinjaGear)
            .AddIngredient(ModContent.ItemType<Super_Turbo_Reel>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
    }
}
