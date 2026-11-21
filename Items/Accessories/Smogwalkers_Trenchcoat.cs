using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back, EquipType.Front)]
	public class Smogwalkers_Trenchcoat : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public float? Damage(Player player, NPC target) {
			bool isBoss = target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type];
			if (player.OriginPlayer().blizzardwalkerActiveTime < Blizzardwalkers_Jacket.max_active_time) {
				return isBoss ? 0.008f : 0.08f;
			}
			return isBoss ? 0.005f : 0.05f;
		}
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Blizzardwalkers_Jacket)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Scrap_Compactor)}.Tooltip").WithFormatArgs("40"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Scrappickers_Glove)}.Tooltip"),
				base.Tooltip
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 4);
			Item.master = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			player.autoReuseGlove = true;
			player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.12f;
			oP.scrapCompactor = true;
			oP.destructiveClaws = true;
			oP.blizzardwalkerJacket = true;
			oP.blizzardwalkerJacketVisual = !hideVisual;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Blizzardwalkers_Jacket>()
			.AddIngredient<Scrappickers_Glove>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
