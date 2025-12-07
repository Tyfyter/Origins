using Microsoft.Xna.Framework;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    public class Chambersite_Ore : OriginTile {
		public static List<int> chambersiteTiles = [];
		public virtual int StoneType => TileID.Stone;
		public virtual Color MapColor => new Color(128, 128, 128);
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMerge[Type][StoneType] = true;
			Main.tileMerge[StoneType][Type] = true;
			TileID.Sets.Ore[Type] = true;
			AddMapEntry(MapColor);
			MinPick = 65;
			MineResist = 2f;
			RegisterItemDrop(ItemType<Chambersite_Item>());
			chambersiteTiles.Add(Type);
		}
	}
	public class Chambersite_Ore_Ebonstone : Chambersite_Ore {
		public override int StoneType => TileID.Ebonstone;
		public override Color MapColor => new Color(109, 90, 128);
	}
	public class Chambersite_Ore_Crimstone : Chambersite_Ore {
		public override int StoneType => TileID.Crimstone;
		public override Color MapColor => new Color(128, 44, 45);
	}
	public class Chambersite_Ore_Defiled_Stone : Chambersite_Ore {
		public override int StoneType => TileType<Defiled_Stone>();
		public override Color MapColor => new Color(200, 200, 200);
	}
	public class Chambersite_Ore_Riven_Flesh : Chambersite_Ore {
		public override int StoneType => TileType<Spug_Flesh>();
		public override Color MapColor => new Color(0, 125, 200);
	}
}
