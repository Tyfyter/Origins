using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Filter_Breather : Gas_Mask {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.LightRed;
			Item.value += Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			base.UpdateEquip(player);
			player.OriginPlayer().rebreather = true;
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) { }
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Gas_Mask>()
			.AddIngredient<Rebreather>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
