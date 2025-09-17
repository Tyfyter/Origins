using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Limestone {
	public class Limestone_Furniture : FurnitureSet<Limestone_Item> {
		public override Color MapColor => new(180, 172, 134);
		public override int DustType => DustID.Sand;
		public override bool LanternSway => false;
		public override bool ChandelierSway => false;
		public override void SetupTile(ModTile tile) {
			if (tile is FurnitureSet_Bookcase) OriginsSets.Tiles.MultitileCollisionOffset[tile.Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if (tile.TileFrameX / 18 != 1) y += 14;
		}
	}
}
