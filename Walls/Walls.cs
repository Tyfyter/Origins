using Origins.Tiles.Other;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Walls {
    public class Defiled_Grass_Wall : OriginsWall {
        public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
        public override Color MapColor => new Color(185, 185, 185);
		public override bool CanBeReplacedByWallSpread => false;
		public override int DustType => Defiled_Wastelands.DefaultTileDust;
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
		public override int DustType => Riven_Hive.DefaultTileDust;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Grass[Type] = true;
			Main.wallBlend[Type] = WallID.Grass;
		}
	}
	public class Ashen_Grass_Wall : OriginsWall {
		public override string Texture => typeof(Defiled_Grass_Wall).GetDefaultTMLName();
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		public override Color MapColor => FromHexRGB(0x463C54);
		public override bool CanBeReplacedByWallSpread => false;
		public override int DustType => Ashen_Biome.DefaultTileDust;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WallID.Sets.Conversion.Grass[Type] = true;
			Main.wallBlend[Type] = WallID.Grass;//what wall type this wall is considered to be when blending
		}
	}
	public class Batholith_Wall : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		public override Color MapColor => new Color(28, 22, 31);
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => ModContent.ItemType<Batholith_Item>();
		public override int DustType => DustID.t_Granite;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallBlend[Type] = WallID.Stone;
		}
	}
	public class Chambersite_Gemspark_Wall_On : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Safe;
		public override Color MapColor => new(49, 184, 191);
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => ModContent.ItemType<Chambersite_Gemspark_Item>();
		public override int DustType => DustID.GemEmerald;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallBlend[Type] = WallID.TopazGemspark;
			Main.wallLight[Type] = true;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			const float strength = 1f;
			r = 0.216f * strength;
			g = 0.800f * strength;
			b = 0.831f * strength;
		}
	}
	public class Chambersite_Gemspark_Wall_Off : OriginsWall {
		public override WallVersion WallVersions => WallVersion.Safe;
		public override Color MapColor => new(15, 67, 69);
		public override bool CanBeReplacedByWallSpread => false;
		public override int TileItem => ModContent.ItemType<Chambersite_Gemspark_Item>();
		public override int DustType => DustID.GemEmerald;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallBlend[Type] = WallID.TopazGemspark;
		}
	}
}
