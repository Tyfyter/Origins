using Terraria;
using Origins.Items.Other.Critters;
using Terraria.ID;

namespace Origins.Tiles.Other {
	public class Chambersite_Bunny_Cage : CageBase {
		public override int LidType => 2;
		public override int[] FrameIndexArray => Main.bunnyCageFrame;
		public override CageKinds CageKind => CageKinds.BigCage;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Chambersite_Bunny_Item>()
				.AddIngredient(ItemID.Terrarium)
				.Register();
			};
		}
	}
}