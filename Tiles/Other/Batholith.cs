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
			AddMapEntry(new Color(35, 10, 10));
			mergeID = TileID.Stone;
			MinPick = 250;
			MineResist = 2;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
	}
	public class Batholith_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Batholith>();
		}
	}
}
