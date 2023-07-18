using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Large_Foliage : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));
			HitSound = Origins.Sounds.DefiledIdle;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);

			TileObjectData.newTile.AnchorValidTiles = new int[]{
				ModContent.TileType<Defiled_Grass>()
			};

			TileObjectData.addTile(Type);
			//soundType = SoundID.Grass;
		}

		public override bool Drop(int i, int j)/* tModPorter Note: Removed. Use CanDrop to decide if an item should drop. Use GetItemDrops to decide which item drops. Item drops based on placeStyle are handled automatically now, so this method might be able to be removed altogether. */ {
			return false;
		}
	}
}
