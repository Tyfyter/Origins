using Terraria;
using Origins.Items.Other.Critters;

namespace Origins.Tiles.Other {
	public class Chambersite_Bunny_Cage : CageBase<Chambersite_Bunny_Item> {
		public override int LidType => 2;
		public override int[] FrameIndexArray => Main.bunnyCageFrame;
		public override CageKinds CageKind => CageKinds.BigCage;
	}
}