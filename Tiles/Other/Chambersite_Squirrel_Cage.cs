using Origins.Items.Other.Critters;
using Terraria.ID;
using Terraria;

namespace Origins.Tiles.Other {
	public class Chambersite_Squirrel_Cage : CageBase {
		public override int LidType => 2;
		public override int[] FrameIndexArray => Main.squirrelCageFrame;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Chambersite_Squirrel_Item>()
				.AddIngredient(ItemID.Terrarium)
				.Register();
			};
		}
	}
}