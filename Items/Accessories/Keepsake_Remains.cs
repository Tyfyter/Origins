using Origins.Buffs;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Keepsake_Remains : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.1f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			"Combat",
			"Torn",
			"TornSource",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 28);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
			player.GetArmorPenetration(DamageClass.Generic) += 5;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.SharkToothNecklace)
			.AddIngredient(ModContent.ItemType<Symbiote_Skull>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
