using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dusk {
	public class Dusk_Light : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(95, 17, 125));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			float m = 0.5f;
			r = 37.2f * m;
			g = 6.7f * m;
			b = 49.2f * m;
		}
	}
	public class Dusk_Light_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Dusk_Light>();
		}
	}
}
