using Origins.Buffs;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Scavenger_Bag : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.1f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.SummonBoostAcc,
			WikiCategories.GenericBoostAcc
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
			CreateRecipe()
			.AddIngredient(ItemID.PapyrusScarab)
			.AddIngredient(ModContent.ItemType<Keepsake_Remains>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
