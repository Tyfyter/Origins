using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Boomerang_Magnet : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"MeleeBoostAcc",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.TreasureMagnet)
			.AddIngredient(ModContent.ItemType<Turbo_Reel>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().boomerangMagnet = true;
		}
	}
}
