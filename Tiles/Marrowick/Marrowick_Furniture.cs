using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Marrowick {
	public class Marrowick_Furniture : FurnitureSet<Marrowick_Item> {
		public override Color MapColor => new(245, 225, 143);
		public override int DustType => DustID.TintablePaint;
		public override Vector3 LightColor {
			get {
				Vector3 color = OriginExtensions.TorchColor(TorchID.Torch);
				color.X = 0f;
				return color;
			}
		}
		public override bool LanternSway => false;
		public override bool ChandelierSway => false;
		public override void SetupTile(ModTile tile) {
			if (tile is FurnitureSet_Bookcase) OriginsSets.Tiles.MultitileCollisionOffset[tile.Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if (tile.TileFrameX / 18 != 1) height = -1600;
		}
	}
}
