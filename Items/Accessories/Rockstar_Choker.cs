using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Rockstar_Choker : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister("ItemTooltip.SharkToothNecklace"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Lightning_Ring)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Makeover_Choker)}.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().lightningRing = true;
			player.GetArmorPenetration(DamageClass.Generic) += 5;
			player.longInvince = true;
			Mysterious_Spray.EquippedEffect(player);
			if (!hideVisual) {
				Mysterious_Spray.VanityEffect(player);
			}
		}
		public override void UpdateVanity(Player player) {
			Mysterious_Spray.VanityEffect(player);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Shock_Collar>()
			.AddIngredient<Makeover_Choker>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
