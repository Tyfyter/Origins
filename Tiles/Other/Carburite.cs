using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    public class Carburite : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			ItemDrop = ItemType<Carburite_Item>();
			AddMapEntry(new Color(110, 57, 33));
			MinPick = 55;
			MineResist = 3;
		}
	}
	public class Carburite_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Carburite");
			Tooltip.SetDefault("'An organic mineral that reacts to compounds'");
			SacrificeTotal = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.value = Item.sellPrice(silver: 1);
			Item.createTile = TileType<Carburite>();
		}
	}
}
