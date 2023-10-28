using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Origins.Tiles {
	public interface IGlowingModTile {
		AutoCastingAsset<Texture2D> GlowTexture { get; }
		Color GlowColor { get; }
		void FancyLightingGlowColor(Tile tile, ref Vector3 color) => color = GlowColor.ToVector3();
	}
	public interface IGlowingModPlant {
		void FancyLightingGlowColor(Tile tile, ref Vector3 color);
	}
}
