using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Marrowick_Flange : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.Origin = new Point16(0, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height - 1).Concat([18]).ToArray();

			TileObjectData.newTile.RandomStyleRange = 3;

			TileObjectData.addTile(Type);

			DustType = DustID.TintablePaint;
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			yield return new(ModContent.ItemType<Marrowick_Item>(), Main.rand.Next(6, 15));
		}
	}
	public class Large_Marrowick_Flange : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Height = 9;
			TileObjectData.newTile.Origin = new Point16(0, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height - 1).Concat([18]).ToArray();

			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;

			TileObjectData.addTile(Type);

			DustType = DustID.TintablePaint;
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			yield return new(ModContent.ItemType<Marrowick_Item>(), Main.rand.Next(8, 20));
		}
	}
}
