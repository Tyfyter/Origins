using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using ReLogic.Content;
using Terraria.ModLoader;
namespace Origins.Tiles.Defiled {
	public class Defiled_Cactus : ModCactus, ICustomWikiStat, INoSeperateWikiPage {
        public string[] Categories => [
            WikiCategories.Plant
        ];
        public override void SetStaticDefaults() {
			GrowsOnTileId = [ModContent.TileType<Defiled_Sand>()];
		}
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>(typeof(Defiled_Cactus).GetDefaultTMLName());
		public override Asset<Texture2D> GetFruitTexture() => ModContent.Request<Texture2D>(typeof(Defiled_Cactus).GetDefaultTMLName() + "_Fruit");// + "_Fruit"
		public bool ShouldHavePage => false;
	}
}
