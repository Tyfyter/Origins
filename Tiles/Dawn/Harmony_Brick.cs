using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dawn {
	public class Harmony_Brick : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ItemType<Harmony_Brick_Item>();
			AddMapEntry(new Color(150, 30, 75));
		}
	}
	public class Harmony_Brick_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Harmony Brick");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CobaltBrick);
			Item.createTile = TileType<Harmony_Brick>();
		}
	}
}
