using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Locket_Necklace : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 32);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.PanicNecklace)
			.AddIngredient(ModContent.ItemType<Comb>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			player.GetModPlayer<OriginPlayer>().guardedHeart = true;
		}
	}
}
