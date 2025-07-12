using Terraria;
using Origins.Items.Other.Critters;
using Terraria.ID;

namespace Origins.Tiles.Other {
	public class Cicada_3301_Cage : CageBase {
		public override int LidType => 3;
		public override int[] FrameIndexArray => Main.bunnyCageFrame;// new int[Main.cageFrames];
		//private readonly int[] FrameCounter = new int[Main.cageFrames];
		public override CageKinds CageKind => CageKinds.SmallCage;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Cicada_3301_Item>()
				.AddIngredient(ItemID.Terrarium)
				.Register();
			};
		}
		public override void ExtraAnimate() {/*
			for (int k = 0; k < Main.cageFrames; k++) {
				if (++FrameCounter[k] >= Main.rand.Next(5, 15)) {
					FrameCounter[k] = 0;
					if (++FrameIndexArray[k] >= 24)
						FrameIndexArray[k] = 0;
				}
			}*/
		}
	}
}