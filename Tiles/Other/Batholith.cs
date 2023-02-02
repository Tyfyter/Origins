using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Batholith : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.Stone[Type] = true;
			ItemDrop = ItemType<Batholith_Item>();
			AddMapEntry(new Color(35, 10, 10));
			mergeID = TileID.Stone;
			MinPick = 225; //modify to 250 when we've implement post-ml
			MineResist = 2;
		}
	}
	public class Batholith_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Batholith");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Batholith>();
		}
	}
}
