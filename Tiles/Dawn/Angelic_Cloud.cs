using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dawn {
	public class Angelic_Cloud : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			ItemDrop = ItemType<Angelic_Cloud_Item>();
			AddMapEntry(new Color(150, 150, 20));
			mergeID = TileID.Cloud;
		}
	}
	public class Angelic_Cloud_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Angelic Cloud");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Cloud);
			Item.createTile = TileType<Angelic_Cloud>();
		}
	}
}
