﻿using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Pocket_Dimension_Monolith : MonolithBase, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsEnabled(tile)) color = Vector3.Max(color, new Vector3(0.394f));
		}
		public override int Frames => 7;
		public override Color MapColor => new(157, 157, 157);
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
		}
		public override void ApplyEffect() {
			if (OriginPlayer.LocalOriginPlayer is not null) OriginPlayer.LocalOriginPlayer.ShimmerConstructMonolith = true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.01f;
		}
		public override void OnLoad() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}
