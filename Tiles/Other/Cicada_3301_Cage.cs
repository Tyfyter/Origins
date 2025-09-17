using Terraria;
using Origins.Items.Other.Critters;

namespace Origins.Tiles.Other {
	public class Cicada_3301_Cage : CageBase<Cicada_3301_Item> {
		public override int LidType => 3;
		public override int[] FrameIndexArray { get; } = new int[Main.cageFrames];
		public override CageKinds CageKind => CageKinds.SmallCage;
		public override void ExtraAnimate() {
			for (int k = 0; k < Main.cageFrames; k++) {
				if (++FrameCounter[k] >= Main.rand.Next(10, 30)) {
					FrameCounter[k] = 0;
					if (++FrameIndexArray[k] >= 20)
						FrameIndexArray[k] = 0;
				}
			}
		}
	}
}