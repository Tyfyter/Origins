using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dawn {
	public class Eden_Wood : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ItemType<Eden_Wood_Item>();
			AddMapEntry(new Color(150, 40, 40));
			mergeID = TileID.WoodBlock;
		}
	}
	public class Eden_Wood_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Eden Wood");
			// Tooltip.SetDefault("'A wood too sacred to chop'");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Wood);
			Item.createTile = TileType<Eden_Wood>();
		}
	}
}
