using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Encrusted_Brick : OriginTile, ICustomWikiStat {
        public string[] Categories => [
            "Brick"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(70, 110, 177));
			DustType = DustID.Astra;
		}
	}
	public class Encrusted_Brick_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Encrusted_Brick>());
		}
	}
}
