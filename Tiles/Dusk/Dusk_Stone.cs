using Microsoft.Xna.Framework;
using Origins.Core;
using PegasusLib;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dusk {
	public class Dusk_Stone : OriginTile {
		public string[] Categories => [
			"Stone"
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
