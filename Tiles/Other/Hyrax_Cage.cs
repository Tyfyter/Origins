using Terraria;
using Origins.Items.Other.Critters;

namespace Origins.Tiles.Other {
	public class Hyrax_Cage : CageBase<Hyrax_Item> {
		public override int LidType => 0;
		public override int[] FrameIndexArray { get; } = new int[Main.cageFrames];
		public override CageKinds CageKind => CageKinds.BigCage;
		public override void ExtraAnimate() {
			for (int k = 0; k < Main.cageFrames; k++) {
				int extraTime = 0;
				if (FrameIndexArray[k] == 0) extraTime = Main.rand.Next(30, 40);
				if (FrameIndexArray[k] == 16) extraTime = Main.rand.Next(10, 20);
				if (FrameIndexArray[k] == 18) extraTime = Main.rand.Next(40, 70);
				if (++FrameCounter[k] >= Main.rand.Next(2, 6) + extraTime) {
					FrameCounter[k] = 0;
					if (++FrameIndexArray[k] >= 30)
						FrameIndexArray[k] = 0;
				}
			}
		}
	}
}