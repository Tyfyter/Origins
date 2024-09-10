using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Water;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Riven_Fountain : WaterFountainBase<Riven_Water_Style>, IGlowingModTile {
		readonly AutoLoadingAsset<Texture2D> glowTexture;
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public Color GlowColor => new Color(196, 196, 196, 100);
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsEnabled(tile)) {
				color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
			}
		}
		public Riven_Fountain() : base() {
			glowTexture = Texture + "_Glow";
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Riven_Fountain_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"WaterFountain"
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Riven_Fountain>());
			Item.value = Item.buyPrice(gold: 4);
		}
	}
}
