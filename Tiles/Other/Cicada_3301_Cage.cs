using Terraria;
using Origins.Items.Other.Critters;
using Terraria.ID;

namespace Origins.Tiles.Other {
	public class Cicada_3301_Cage : CageBase {
		public override string Texture => "Origins/Tiles/Other/Amoeba_Buggy_Cage";
		public override int LidType => 0;
		public override int[] FrameIndexArray => Main.snailCageFrame;
		public override CageKinds CageKind => CageKinds.SmallCage;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Cicada_3301_Item>()
				.AddIngredient(ItemID.Terrarium)
				.Register();
			};
		}
	}
}