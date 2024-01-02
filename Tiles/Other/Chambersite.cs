using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
    public class Chambersite : OriginTile {
		public override void SetStaticDefaults() { //TODO: gemstone properties
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			AddMapEntry(new Color(10, 60, 25));
			MinPick = 35;
			MineResist = 3;
		}
	}
    public class Chambersite_Item : ModItem {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 15;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneBlock);
            Item.value = Item.sellPrice(silver: 13);
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 9999;
            //Item.createTile = ModContent.TileType<Chmabersite>();
        }
    }
}
