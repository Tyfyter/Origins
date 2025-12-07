using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    public class Super_Turbo_Reel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.hasMagiluminescence = true;
			player.GetModPlayer<OriginPlayer>().turboReel2 = true;
			DelegateMethods.v3_1 = new Vector3(0.9f, 0.8f, 0.5f);
			Utils.PlotTileLine(player.Center, player.Center + player.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
			Utils.PlotTileLine(player.Left, player.Right, 20f, DelegateMethods.CastLightOpen);
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ItemID.Magiluminescence)
            .AddIngredient(ModContent.ItemType<Turbo_Reel>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
    }
}
