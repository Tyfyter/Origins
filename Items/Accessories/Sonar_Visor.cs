using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Sonar_Visor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.advancedImaging = true;
			originPlayer.sonarVisor = true;
			player.buffImmune[BuffID.Confused] = true;

			player.GetCritChance(DamageClass.Generic) += 10;
			player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.2f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.UltrabrightHelmet)
			.AddIngredient(ModContent.ItemType<Advanced_Imaging>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
