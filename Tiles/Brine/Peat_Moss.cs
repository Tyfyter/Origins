using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	[LegacyName("Peat_Moss")]
	public class Peat_Moss_Tile : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ItemType<Peat_Moss>();
			AddMapEntry(new Color(18, 160, 56));
			HitSound = SoundID.Dig;
		}
	}
}
