using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Plague_Doctor_Mask : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 24);
			Item.defense = 2;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			oP.medicalMask = true;
			oP.gasMask = true;
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) {
			player.OriginPlayer().gasMaskDye = dye;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Gas_Mask>()
			.AddIngredient<Medical_Mask>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
