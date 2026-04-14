using Terraria.ID;

namespace Origins.Walls; 
public class Truss_Wall : OriginsWall {
	public override WallVersion WallVersions => WallVersion.Placed_Unsafe | WallVersion.Safe;
	public override Color MapColor => new Color(190, 145, 112);
	public override int DustType => DustID.Copper;
	public override bool CanBeReplacedByWallSpread => false;
	public override void SetStaticDefaults() {
		base.SetStaticDefaults();
		WallID.Sets.AllowsWind[Type] = true;
		WallID.Sets.Transparent[Type] = true;
	}
}
