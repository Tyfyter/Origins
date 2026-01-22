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
				switch (FrameIndexArray[k]) {
					case 0:
					extraTime = Main.rand.Next(30, 40);
					break;

					case 16:
					extraTime = Main.rand.Next(10, 20);
					if (!Main.rand.NextBool(3)) continue;
					break;

					case 18:
					extraTime = Main.rand.Next(40, 70);
					if (FrameCounter[k] == 0 && Main.rand.NextBool(3)) FrameCounter[k] = -1;
					break;
				}
				if (FrameCounter[k] < 0) {
					if (--FrameCounter[k] < -(Main.rand.Next(2, 6) + extraTime)) {
						FrameCounter[k] = -1;
						if (--FrameIndexArray[k] < 17) FrameCounter[k] = 0;
					}
					continue;
				}
				if (++FrameCounter[k] >= Main.rand.Next(2, 6) + extraTime) {
					FrameCounter[k] = 0;
					if (++FrameIndexArray[k] >= 30)
						FrameIndexArray[k] = 0;
				}
			}
		}
	}
}