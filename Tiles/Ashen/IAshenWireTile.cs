using Origins.Items.Tools.Wiring;
using Terraria;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public interface IAshenWireTile {
		public bool IsPowered(int i, int j) => AshenWireTile.DefaultIsPowered(i, j);
		public void UpdatePowerState(int i, int j, bool powered);
		public void HitWire(int i, int j);
	}
	public static class AshenWireTile {
		public static bool DefaultIsPowered(int i, int j) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					if (Main.tile[left + x, top + y].Get<Ashen_Wire_Data>().AnyPower) return true;
				}
			}
			return false;
		}
		public delegate ref short GetFrame(Tile tile);
		public static void DefaultUpdatePowerState(int i, int j, bool powered, GetFrame frame, int frameSize, bool invertOrder = false, bool quiet = false) {
			bool wasPowered = frame(Main.tile[i, j]) >= frameSize;
			powered ^= invertOrder;
			if (powered == wasPowered) return;
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					ref short useFrame = ref frame(Main.tile[left + x, top + y]);
					useFrame = (short)(useFrame % frameSize + (powered ? frameSize : 0));
				}
			}
			if (!NetmodeActive.SinglePlayer && !quiet) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
		}
	}
}
