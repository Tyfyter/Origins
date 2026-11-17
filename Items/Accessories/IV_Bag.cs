using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class IV_Bag : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			oP.pacemaker = true;
			oP.drippinIt = true;
			player.lifeRegen += 1;
			oP.plasmaPhial = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Pacemaker>()
			.AddIngredient<Plasma_Bag>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
