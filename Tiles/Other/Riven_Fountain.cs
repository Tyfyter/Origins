using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;

namespace Origins.Tiles.Other {
	public class Riven_Fountain : WaterFountainBase<Riven_Hive>, IGlowingModTile {
		public override void SetBiomeActive() => Riven_Hive.forcedBiomeActive = true;
		readonly AutoLoadingAsset<Texture2D> glowTexture;
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public Color GlowColor => new(196, 196, 196, 100);
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsEnabled(tile)) {
				color.DoFancyGlow(new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue(), tile.TileColor);
			}
		}
		public Riven_Fountain() : base() {
			glowTexture = Texture + "_Glow";
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 8 - (frame + 1) % Frames) {
				frameCounter = 0;
				frame = (frame + 1) % Frames;
			}
		}
		public override void OnLoad() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}
