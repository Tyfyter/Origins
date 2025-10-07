using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Ashen_Cactus : ModCactus, IGlowingModPlant, ICustomWikiStat, INoSeperateWikiPage {
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Ashen_Cactus).GetDefaultTMLName() + "_Glow";
		public string[] Categories => [
            "Plant"
        ];
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
				case (0, 1):
				case (5, 1):
				color = new Vector3(0.912f, 0.879f, 0.394f);
				break;
			}
		}
		public override void SetStaticDefaults() {
			GrowsOnTileId = [ModContent.TileType<Sootsand>()];
			GlowPaintKey = CustomTilePaintLoader.CreateKey();
		}
		public static CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>(typeof(Ashen_Cactus).GetDefaultTMLName());
		public override Asset<Texture2D> GetFruitTexture() => ModContent.Request<Texture2D>(typeof(Ashen_Cactus).GetDefaultTMLName() + "_Fruit");
		public bool ShouldHavePage => false;
	}
}
