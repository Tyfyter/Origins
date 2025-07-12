using Terraria;
using Origins.Items.Other.Critters;
using Terraria.ID;

namespace Origins.Tiles.Other {
	public class Amoeba_Buggy_Cage : CageBase {
		public override int LidType => 3;
		public override int[] FrameIndexArray => Main.bunnyCageFrame;// new int[Main.cageFrames];
		//private readonly int[] FrameCounter = new int[Main.cageFrames];
		public override CageKinds CageKind => CageKinds.SmallCage;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Amoeba_Buggy_Item>()
				.AddIngredient(ItemID.Terrarium)
				.Register();
			};
		}
		public override void ExtraAnimate() {/*
			//will fix later unless someone wants to fix this
			string tmp = string.Empty;
			for (int k = 0; k < Main.cageFrames; k++) {
				if (++FrameCounter[k] >= Main.rand.Next(5, 15)) {
					FrameCounter[k] = 0;
					if (++FrameIndexArray[k] >= 24)
						FrameIndexArray[k] = 0;
				}
				if (string.IsNullOrEmpty(tmp)) tmp = $"F: {FrameIndexArray[k]}, C: {FrameCounter[k]}";
				else tmp += $"\nF: {FrameIndexArray[k]}, C: {FrameCounter[k]}";
			}
			//tmp += $"\nslug:\n{Main.slugCageFrame[0,0]}";
			//for (int i = 1; i < Main.cageFrames; i++) tmp += $", {Main.slugCageFrame[0,i]}";
			Main.NewText(tmp);*/
		}
	}
}