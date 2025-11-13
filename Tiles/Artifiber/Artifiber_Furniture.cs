using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Artifiber {
	public class Artifiber_Furniture : FurnitureSet<Artifiber_Item> {
		public override Color MapColor => new(44, 39, 58);
		public override int DustType => DustID.WoodFurniture;
		public override Vector3 LightColor {
			get {
				Vector3 color = default;
				TorchID.TorchColor(TorchID.Torch, out color.X, out color.Y, out color.Z);
				color.Y *= 0.8f;
				color.Z *= 0.6f;
				return color;
			}
		}
		public override void SetupTile(ModTile tile) {
			if (tile is FurnitureSet_Bookcase) OriginsSets.Tiles.MultitileCollisionOffset[tile.Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			//if (tile.TileFrameX / 18 != 1) y += 14;
		}
		public override void ChandelierSwayParams(LightFurnitureBase tile, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			// Vanilla chandeliers all share these parameters.
			overrideWindCycle = 1f;
			windPushPowerY = 0;

			overrideWindCycle = null;
			windPushPowerY = -1f;
			dontRotateTopTiles = true;
		}
		public override void ChandelierFlameData(LightFurnitureBase tile, int i, int j, ref TileDrawing.TileFlameData tileFlameData) {
			ulong flameSeed = Main.TileFrameSeed ^ (ulong)((long)i << 32 | (uint)j);

			tileFlameData.flameTexture = tile.flameTexture.Value;
			tileFlameData.flameSeed = flameSeed;

			tileFlameData.flameCount = 7;
			tileFlameData.flameColor = new Color(100, 100, 100, 0);
			tileFlameData.flameRangeXMin = -10;
			tileFlameData.flameRangeXMax = 11;
			tileFlameData.flameRangeYMin = -10;
			tileFlameData.flameRangeYMax = 1;
			tileFlameData.flameRangeMultX = 0.15f;
			tileFlameData.flameRangeMultY = 0.35f;
		}
		public override void LanternSwayParams(LightFurnitureBase tile, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			ChandelierSwayParams(tile, i, j, ref overrideWindCycle, ref windPushPowerX, ref windPushPowerY, ref dontRotateTopTiles, ref totalWindMultiplier, ref glowTexture, ref glowColor);
			dontRotateTopTiles = false;
		}
		public override void LanternFlameData(LightFurnitureBase tile, int i, int j, ref TileDrawing.TileFlameData tileFlameData) {
			ChandelierFlameData(tile, i, j, ref tileFlameData);
		}
	}
}
