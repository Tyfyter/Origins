using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Shock_Collar : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister("ItemTooltip.SharkToothNecklace"),
				Language.GetOrRegister("Mods.Origins.Items.Lightning_Ring.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().lightningRing = true;
			player.GetArmorPenetration(DamageClass.Generic) += 5;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.SharkToothNecklace)
			.AddIngredient<Lightning_Ring>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
