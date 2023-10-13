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
			RegisterItemDrop(ItemType<Peat_Moss>());
			AddMapEntry(new Color(18, 160, 56));
			HitSound = SoundID.Dig;
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j - 1).HasTile) {
				if (TileObject.CanPlace(i, j - 1, TileType<Brineglow_Vine>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
		}
	}
}
