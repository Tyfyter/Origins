using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Gaming_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"MeleeBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.kbGlove = true;
			player.meleeScaleGlove = true;
			player.OriginPlayer().resizingGlove = true;
			player.autoReuseGlove = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.PowerGlove)
			.AddIngredient<Resizing_Glove>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
