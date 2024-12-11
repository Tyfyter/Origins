using Microsoft.Xna.Framework;
using Origins.Tiles.Cubekon;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dawn {
	public class Harmony_Brick : OriginTile {
        public string[] Categories => [
            "Brick"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(150, 30, 75));
		}
	}
	public class Harmony_Brick_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Harmony_Brick>());
		}
	}
}
