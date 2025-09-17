using Humanizer;
using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static PegasusLib.TileUtils;

namespace Origins.Tiles.Riven {
	public class Marrowick_Coral : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			int[] validTiles = [
				ModContent.TileType<Riven_Grass>(),
				ModContent.TileType<Spug_Flesh>()
			];

			TileObjectData.newTile.AnchorValidTiles = [..validTiles,
				TileID.Stone,
				TileID.Grass
			];

			TileObjectData.newTile.RandomStyleRange = 3;

			TileObjectData.addTile(Type);

			DustType = DustID.TintablePaint;
		}
		public override bool CanDrop(int i, int j) => true;
		public override void RandomUpdate(int i, int j) {
			if (Main.tile[i, j].TileFrameY / 36 >= 2) return;
			if (TileObjectData.IsTopLeft(Main.tile[i, j]) && WorldGen.genRand.NextBool(3)) {
				for (int y = 0; y < 2; y++) {
					for (int x = 0; x < 3; x++) {
						Framing.GetTileSafely(i + x, j + y).TileFrameY += 36;
					}
				}
			}
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			yield return new(ModContent.ItemType<Marrowick_Item>(), Main.rand.Next(4, 10));
			switch (Main.tile[i, j].TileFrameY / 36) {
				case 1:
				yield return new(ModContent.ItemType<Bud_Barnacle>(), Main.rand.Next(1, 3));
				break;

				case 2:
				yield return new(ModContent.ItemType<Bud_Barnacle>(), Main.rand.Next(3, 7));
				break;
			}
		}
	}
}
