using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Limestone {
	public class Limestone_Furniture : FurnitureSet<Limestone_Item> {
		public override Color MapColor => new(180, 172, 134);
		public override int DustType => DustID.Sand;
		public override void SetupTile(ModTile tile) {
			if (tile is FurnitureSet_Bookcase) OriginsSets.Tiles.MultitileCollisionOffset[tile.Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(short frameX, ref float y, ref int height) {
			if (frameX / 18 != 1) y += 14;
		}
	}
}
