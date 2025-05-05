using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Party_Ring_Balloon : Donor_Wristband, ICustomWikiStat {
		public new string[] Categories => [
			"Vitality"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.value = Item.sellPrice(gold: 7, silver: 60);
			Item.rare = ItemRarityID.Lime;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BalloonHorseshoeHoney)
			.AddIngredient<Donor_Wristband>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			base.UpdateEquip(player);
			player.jumpBoost = true;
			player.honeyCombItem = Item;
			player.noFallDmg = true;
			player.hasLuck_LuckyHorseshoe = true;
		}
	}
}
