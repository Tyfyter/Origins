using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Endowood : OriginTile {
        public string[] Categories => [
			WikiCategories.Plant
		];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(40, 10, 40));
			mergeID = TileID.WoodBlock;
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Endowood>());
		}
    }
}
