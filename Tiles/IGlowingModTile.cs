using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using ReLogic.Content;
using Terraria;

namespace Origins.Tiles {
	public interface IGlowingModTile {
		AutoCastingAsset<Texture2D> GlowTexture { get; }
		CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		Color GlowColor { get; }
		void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, GlowColor.ToVector3());
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
		(float r, float g, float b) LightEmission { get; }
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
	}
}
