using Microsoft.Xna.Framework;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Walls {
    public class Defiled_Grass_Wall : OriginsWall {
        public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
        public override Color MapColor => new Color(185, 185, 185);
		public override bool CanBeReplacedByWallSpread => false;
		public override void SetStaticDefaults() {
            base.SetStaticDefaults();
			WallID.Sets.Conversion.Grass[Type] = true;
            Main.wallBlend[Type] = WallID.Grass;//what wall type this wall is considered to be when blending
        }
    }
    public class Riven_Grass_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		public override Color MapColor => new Color(40, 140, 200);
		public override bool CanBeReplacedByWallSpread => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Grass[Type] = true;
			Main.wallBlend[Type] = WallID.Grass;
		}
	}
	public class Batholith_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		public override Color MapColor => new Color(28, 22, 31);
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => ModContent.ItemType<Batholith_Item>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallBlend[Type] = WallID.Stone;
		}
	}
}
