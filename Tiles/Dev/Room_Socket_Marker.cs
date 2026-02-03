using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Dev {
	public class Room_Socket_Marker : ModTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this, true));
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.CanBeSloped[Type] = true;
		}
		public override bool Slope(int i, int j) {
			Tile tile = Main.tile[i, j];
			tile.TileFrameX += 18;
			tile.TileFrameX %= 5 * 18;
			return false;
		}
	}
}
