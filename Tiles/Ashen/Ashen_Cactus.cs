using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Tiles.Defiled;
using ReLogic.Content;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Ashen_Cactus : ModCactus, ICustomWikiStat, INoSeperateWikiPage {
        public string[] Categories => [
            "Plant"
        ];
        public override void SetStaticDefaults() {
			GrowsOnTileId = [ModContent.TileType<Sootsand>()];
		}
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>(typeof(Defiled_Cactus).GetDefaultTMLName());
		public override Asset<Texture2D> GetFruitTexture() => ModContent.Request<Texture2D>(typeof(Defiled_Cactus).GetDefaultTMLName() + "_Fruit");// + "_Fruit"
		public bool ShouldHavePage => false;
	}
}
