using Terraria;
using Origins.Items.Other.Critters;
using Terraria.ID;

namespace Origins.Tiles.Other {
	public class Bug_Cage : CageBase {
		public override string Texture => "Origins/Tiles/Other/Amoeba_Buggy_Cage";
		public override int LidType => 3;
		public override int[] FrameIndexArray => Main.bunnyCageFrame;
		public override CageKinds CageKind => CageKinds.SmallCage;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Bug_Item>()
				.AddIngredient(ItemID.Terrarium)
				.Register();
			};
		}
	}
}