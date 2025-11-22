using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Breathing_Unit : Filter_Breather {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.faceSlot = ModContent.GetInstance<Filter_Breather>()?.Item?.faceSlot ?? -1;
			Item.value += Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			base.UpdateEquip(player);
			player.buffImmune[BuffID.Suffocation] = true;
			if (player.OriginPlayer().airTank.TrySet(true)) player.AddMaxBreath(257);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Air_Tank>()
			.AddIngredient<Filter_Breather>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
