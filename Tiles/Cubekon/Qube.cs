using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Cubekon {
    public class Qube : OriginTile {
        public string[] Categories => [
            WikiCategories.OtherBlock
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			AddMapEntry(new Color(10, 255, 80));
			MinPick = 235;
			MineResist = 2;
		}
	}
    public class Qube_Item : ModItem {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Qube>());
			Item.rare = ButterscotchRarity.ID;
        }
    }
}
