using Origins.Items.Other.Critters;
using Terraria;

namespace Origins.Tiles.Other {
	public class Chambersite_Squirrel_Cage : CageBase<Chambersite_Squirrel_Item> {
		public override int LidType => 2;
		public override int[] FrameIndexArray => Main.squirrelCageFrame;
		public override CageKinds CageKind => CageKinds.BigCage;
	}
}