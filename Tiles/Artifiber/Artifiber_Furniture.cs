using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Artifiber {
	public class Artifiber_Furniture : FurnitureSet<Artifiber_Item> {
		public override Color MapColor => new(143, 114, 94);
		public override int DustType => DustID.WoodFurniture;
		public override Vector3 LightColor {
			get {
				Vector3 color = new(1, 0.7f, 0.7f);
				return color;
			}
		}
		public override void SetupTile(ModTile tile) {
			if (tile is FurnitureSet_Bookcase) OriginsSets.Tiles.MultitileCollisionOffset[tile.Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			y += 6;
		}
		public override void ChandelierSwayParams(LightFurnitureBase tile, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			// Vanilla chandeliers all share these parameters.
			overrideWindCycle = 1f;
			windPushPowerY = 0;
			totalWindMultiplier *= 0.2f;
		}
		public override void LanternSwayParams(LightFurnitureBase tile, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			ChandelierSwayParams(tile, i, j, ref overrideWindCycle, ref windPushPowerX, ref windPushPowerY, ref dontRotateTopTiles, ref totalWindMultiplier, ref glowTexture, ref glowColor);
		}
	}
}
