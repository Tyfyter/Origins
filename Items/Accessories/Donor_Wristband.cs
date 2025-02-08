using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Donor_Wristband : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.value = Item.sellPrice(gold: 4, silver: 60);
			Item.rare = ItemRarityID.LightPurple;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CharmofMyths)
			.AddIngredient(ModContent.ItemType<Plasma_Bag>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegen += 2;
			player.GetModPlayer<OriginPlayer>().donorWristband = true;
		}
	}
}
