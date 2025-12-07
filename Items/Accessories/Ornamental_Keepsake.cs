using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Ornamental_Keepsake : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Mithrafin)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Venom_Fang)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Keepsake_Remains)}.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.mithrafin = true;
			originPlayer.venomFang = true;
			originPlayer.symbioteSkull = true;
			player.GetArmorPenetration(DamageClass.Generic) += 5;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Keepsake_Remains>()
			.AddIngredient<Noxious_Mithrafin>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
