using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;

namespace Origins.Tiles {
	public interface IGlowingModTile {
		AutoCastingAsset<Texture2D> GlowTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		Color GlowColor { get; }
		void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color.DoFancyGlow(GlowColor.ToVector3(), tile.TileColor);
		}
	}
	public interface IGlowingModWall {
		AutoCastingAsset<Texture2D> GlowTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		Color GlowColor { get; }
		void FancyLightingGlowColor(Tile tile, ref Vector3 color) => color = GlowColor.ToVector3();
	}
	public interface IGlowingModPlant {
		void FancyLightingGlowColor(Tile tile, ref Vector3 color);
	}
	public interface IGlowingModTree : IGlowingModTile {
		AutoLoadingAsset<Texture2D> TopTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey TopPaintKey { get; set; }
		AutoLoadingAsset<Texture2D> TopGlowTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey TopGlowPaintKey { get; set; }
		AutoLoadingAsset<Texture2D> BranchesTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey BranchesPaintKey { get; set; }
		AutoLoadingAsset<Texture2D> BranchesGlowTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey BranchesGlowPaintKey { get; set; }
		(float r, float g, float b) LightEmission(int i, int j);
	}
	public static class GlowingTileExtensions {
		public static void SetupGlowKeys(this IGlowingModTile self) {
			self.GlowPaintKey = CustomTilePaintLoader.CreateKey();
		}
		public static void SetupGlowKeys(this IGlowingModTree self) {
			self.GlowPaintKey = CustomTilePaintLoader.CreateKey();
			self.TopPaintKey = CustomTilePaintLoader.CreateKey();
			self.TopGlowPaintKey = CustomTilePaintLoader.CreateKey();
		}
		public static Texture2D GetGlowTexture(this IGlowingModTile self, int paintColor) {
			return CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(self.GlowPaintKey, paintColor, self.GlowTexture.asset);
		}
		public static Texture2D GetTopTexture(this IGlowingModTree self, int paintColor) {
			return CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(self.TopPaintKey, paintColor, self.TopTexture);
		}
		public static Texture2D GetTopGlowTexture(this IGlowingModTree self, int paintColor) {
			return CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(self.TopGlowPaintKey, paintColor, self.TopGlowTexture);
		}
		public static void DoFancyGlow(ref this Vector3 fancyLightingCurrentColor, Vector3 color, int paintColor) {
			Vector3 GetColor(int paintColor) {
				switch (paintColor) {
					case PaintID.RedPaint:
					case PaintID.OrangePaint:
					case PaintID.YellowPaint:
					case PaintID.LimePaint:
					case PaintID.GreenPaint:
					case PaintID.TealPaint:
					case PaintID.CyanPaint:
					case PaintID.SkyBluePaint:
					case PaintID.BluePaint:
					case PaintID.PurplePaint:
					case PaintID.VioletPaint:
					case PaintID.PinkPaint:
					return Vector3.Lerp(color, GetColor(paintColor + (PaintID.DeepRedPaint - PaintID.RedPaint)), 0.75f);
					case PaintID.DeepRedPaint:
					return Color.Red.ToVector3();
					case PaintID.DeepOrangePaint:
					return Color.OrangeRed.ToVector3();
					case PaintID.DeepYellowPaint:
					return Color.Yellow.ToVector3();
					case PaintID.DeepLimePaint:
					return Color.Lime.ToVector3();
					case PaintID.DeepGreenPaint:
					return Color.Green.ToVector3();
					case PaintID.DeepTealPaint:
					return Color.Teal.ToVector3();
					case PaintID.DeepCyanPaint:
					return Color.Cyan.ToVector3();
					case PaintID.DeepSkyBluePaint:
					return Color.DeepSkyBlue.ToVector3();
					case PaintID.DeepBluePaint:
					return Color.Blue.ToVector3();
					case PaintID.DeepPurplePaint:
					return Color.Purple.ToVector3();
					case PaintID.DeepVioletPaint:
					return Color.Violet.ToVector3();
					case PaintID.DeepPinkPaint:
					return Color.Pink.ToVector3();

					case PaintID.ShadowPaint:
					case PaintID.BlackPaint:
					return Vector3.Zero;

					case PaintID.GrayPaint:
					case PaintID.WhitePaint:
					return Vector3.One;
					case PaintID.BrownPaint:
					return Color.Brown.ToVector3();
					case PaintID.NegativePaint:
					return Vector3.One - color;
				}
				return color;
			}
			float brigtness = color.Length();
			fancyLightingCurrentColor = Vector3.Max(fancyLightingCurrentColor, GetColor(paintColor).Normalized(out _) * brigtness);
		}
	}
}
