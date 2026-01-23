using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using ReLogic.Content;
using System;
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
		public static void DoFancyGlow(ref this Vector3 fancyLightingCurrentColor, Vector3 color, int paintColor) => DoFancyGlow(
			ref fancyLightingCurrentColor.X, ref fancyLightingCurrentColor.Y, ref fancyLightingCurrentColor.Z,
			color, paintColor
		);
		public static void PaintLight(ref float r, ref float g, ref float b, int paintColor) {
			Vector3 color = new(r, g, b);
			r = 0;
			g = 0;
			b = 0;
			DoFancyGlow(ref r, ref g, ref b, color, paintColor);
		}
		public static void DoFancyGlow(ref float r, ref float g, ref float b, Vector3 color, int paintColor) {
			float brightness = color.Length();
			Vector3 GetColor(int paintColor) {
				switch (paintColor) {
					case PaintID.None:
					return color;

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
					return Vector3.Lerp(color, WorldGen.paintColor(paintColor).ToVector3(), 0.75f);

					case PaintID.NegativePaint:
					return new Vector3(Math.Max(color.X, Math.Max(color.Y, color.Z))) - color;

					case PaintID.GrayPaint:
					brightness *= 0.5f;
					return Vector3.One;
					case PaintID.BlackPaint:
					brightness *= 0.25f;
					return Vector3.One;
					case PaintID.ShadowPaint:
					return Vector3.Zero;
				}
				return WorldGen.paintColor(paintColor).ToVector3();
			}
			color = GetColor(paintColor).Normalized(out _) * brightness;
			Max(ref r, color.X);
			Max(ref g, color.Y);
			Max(ref b, color.Z);
		}
	}
}
