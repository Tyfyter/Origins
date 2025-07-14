using Terraria;
using Origins.Items.Other.Critters;

namespace Origins.Tiles.Other {
	public class Bug_Cage : CageBase<Bug_Item> {
		public override string Texture => "Origins/Tiles/Other/Amoeba_Buggy_Cage";
		public override int LidType => 3;
		public override int[] FrameIndexArray { get; } = new int[Main.cageFrames];
		public override CageKinds CageKind => CageKinds.SmallCage;
		public override void ExtraAnimate() {
			for (int k = 0; k < Main.cageFrames; k++) {
				if (++FrameCounter[k] >= Main.rand.Next(10, 30)) {
					FrameCounter[k] = 0;
					if (++FrameIndexArray[k] >= 24)
						FrameIndexArray[k] = 0;
				}
			}
		}
	}
}