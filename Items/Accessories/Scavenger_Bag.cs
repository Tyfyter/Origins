using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Scavenger_Bag : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"Torn",
			"TornSource",
			"SummonBoostAcc",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 30);
			Item.value = Item.sellPrice(gold: 9);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetKnockback(DamageClass.Summon).Base += 2f;
			player.GetDamage(DamageClass.Summon) += 0.15f;
			player.maxMinions++;
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
			player.GetArmorPenetration(DamageClass.Generic) += 5;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.PapyrusScarab);
			recipe.AddIngredient(ModContent.ItemType<Keepsake_Remains>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
