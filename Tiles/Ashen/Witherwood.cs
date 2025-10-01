using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Witherwood : OriginTile {
		public override string Texture => typeof(Defiled.Endowood).GetDefaultTMLName();
		public string[] Categories => [
            "Plant"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(255, 10, 40));
			mergeID = TileID.WoodBlock;
			DustType = DustID.t_Granite;
		}
	}
	public class Witherwood_Item : ModItem {
		public override string Texture => typeof(Endowood_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Witherwood>());
		}
    }
}
