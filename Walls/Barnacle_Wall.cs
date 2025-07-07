using Origins.World.BiomeData;
using Terraria.ID;

namespace Origins.Walls {
	public class Barnacle_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe;
		public override Color MapColor => new(9, 32, 45);
		public override int DustType => Riven_Hive.DefaultTileDust;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.NewWall1[Type] = true;
		}
	}
}
