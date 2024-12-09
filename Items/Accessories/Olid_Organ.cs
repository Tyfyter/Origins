using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Olid_Organ : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc",
			"ToxicSource"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 24);
			Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.sellPrice(gold: 4);
		}
		public override void UpdateEquip(Player player) {
			player.aggro -= 150;
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			player.GetCritChance(DamageClass.Generic) += 5f;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.venomFang = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.PutridScent)
			.AddIngredient(ModContent.ItemType<Venom_Fang>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
