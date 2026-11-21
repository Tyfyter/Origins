using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
	public class Scrappickers_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Scrap_Compactor)}.Tooltip").WithFormatArgs("40"),
				base.Tooltip
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 28);
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			player.autoReuseGlove = true;
			player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.12f;
			oP.scrapCompactor = true;
			oP.destructiveClaws = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.FeralClaws)
			.AddIngredient<Scrap_Compactor>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
