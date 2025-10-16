using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Sparking_Flame_Barrel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister("Mods.Origins.Items.Danger_Barrel.Tooltip"),
				Language.GetOrRegister("Mods.Origins.Items.Lightning_Ring.Tooltip")
			);
		public override void SetStaticDefaults() {
			Accessory_Glow_Layer.AddGlowMask<Back_Glow_Layer>(Item.backSlot, Texture + "_Back_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 22);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.dangerBarrel = true;
			originPlayer.lightningRing = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Danger_Barrel>()
			.AddIngredient<Lightning_Ring>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
