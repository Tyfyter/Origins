using Origins.Tiles.Brine;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables {
	public class Super_Mojo_Flask : Mojo_Flask {
		public override int FlaskUseCount => 7;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.buffTime = 25;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Mojo_Flask>()
			.AddIngredient(ItemID.SoulofLight, 8)
			.AddIngredient<Brineglow_Item>(8)
			.AddTile<Cleansing_Station>()
			.Register();
	}
}
