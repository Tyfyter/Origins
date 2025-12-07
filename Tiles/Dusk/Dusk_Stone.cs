using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dusk {
	public class Undusk : ILoadable {
		public void Load(Mod mod) {
			WorldFile.OnWorldLoad += WorldFile_OnWorldLoad;
		}
		static void WorldFile_OnWorldLoad() {
			int stone = TileType<Dusk_Stone>();
			int liquid = TileType<Dusk_Stone_Liquid>();
			if (stone == 0) stone = int.MinValue;
			if (liquid == 0) liquid = int.MinValue;
			for (int i = 0; i < Main.maxTilesX; i++) {
				for (int j = 0; j < Main.maxTilesY; j++) {
					Tile tile = Main.tile[i, j];
					if (!tile.HasTile) continue;
					if (tile.TileType == stone) {
						tile.TileType = TileID.Ash;
					} else if (tile.TileType == liquid) {
						tile.HasTile = false;
						tile.LiquidType = LiquidID.Lava;
						tile.LiquidAmount = 255;
					}
				}
			}
		}
		public void Unload() {}
	}
	public class Dusk_Stone : OriginTile {
		public string[] Categories => [
			WikiCategories.Stone
		];
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileType<Dusk_Stone_Liquid>()] = true;
			Main.tileMerge[Type][TileType<Bleeding_Obsidian>()] = true;
			AddMapEntry(new Color(20, 20, 20));
			mergeID = TileID.Stone;
			MinPick = 200;
			HitSound = SoundID.Dig;
			DustType = DustID.t_Granite;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public override void PostSetDefaults() {
			Main.tileNoSunLight[Type] = false;
		}
	}
	public class Dusk_Stone_Liquid : Dusk_Stone {
		public override string Texture => typeof(Dusk_Stone).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileMerge[Type][TileType<Dusk_Stone>()] = true;
		}
	}
}
