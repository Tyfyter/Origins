using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Dev {
	public class Room_Socket_Marker : ModTile {
		public static bool IsSocketMarker(Tile tile) => tile.HasTile && TileLoader.GetTile(tile.TileType) is Room_Socket_Marker;
		public override void Load() {
			Mod.AddContent(new TileItem(this, true));
		}
		public sealed override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.CanBeSloped[Type] = true;
		}
		public sealed override bool Slope(int i, int j) {
			Tile tile = Main.tile[i, j];
			tile.TileFrameX += 18;
			tile.TileFrameX %= 5 * 18;
			return false;
		}
		public sealed override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	}
}
