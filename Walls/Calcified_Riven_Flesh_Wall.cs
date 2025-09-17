using Terraria.ID;

namespace Origins.Walls {
	public class Calcified_Riven_Flesh_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe;
		public override Color MapColor => new(84, 88, 106);
		public override int DustType => DustID.DungeonBlue;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Stone[Type] = true;
			OriginsSets.Walls.RivenWalls[Type] = WallVersion == WallVersion.Natural;
		}
	}
}
