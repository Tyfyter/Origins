using Origins.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core {
	internal class BlockedTiles : ModSystem {
		static Action clear;
		public override void Load() {
			clear = typeof(Main).Assembly.GetType("Terraria.TileData`1").MakeGenericType(typeof(Blocked_Tile_Data)).GetMethod("ClearEverything").CreateDelegate<Action>();
		}
		public override void PostUpdateProjectiles() {
			clear();
			static void Set(Rectangle hitbox, byte mask) {
				Point topLeft = hitbox.TopLeft().ToTileCoordinates();
				Point bottomRight = hitbox.BottomRight().ToTileCoordinates();
				int minX = Utils.Clamp(topLeft.X, 0, Main.maxTilesX - 1);
				int minY = Utils.Clamp(topLeft.Y, 0, Main.maxTilesY - 1);
				int maxX = Utils.Clamp(bottomRight.X, 0, Main.maxTilesX - 1) - minX;
				int maxY = Utils.Clamp(bottomRight.Y, 0, Main.maxTilesY - 1) - minY;
				for (int i = 0; i <= maxX; i++) {
					for (int j = 0; j <= maxY; j++) {
						Main.tile[i + minX, j + minY].Get<Blocked_Tile_Data>().Set(mask);
					}
				}
			}
			foreach (Player player in Main.ActivePlayers) {
				if (player.shimmering || player.dead || player.ghost) continue;
				Set(player.Hitbox, 0b0001);
			}
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.noTileCollide) continue;
				Set(npc.Hitbox, 0b0010);
			}
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (!projectile.tileCollide) continue;
				Set(projectile.Hitbox, 0b0100);
			}
		}
		public static bool Get(int i, int j, byte mask = 0b0011) => Main.tile[i, j].Get<Blocked_Tile_Data>().Get(mask);
		struct Blocked_Tile_Data : ITileData {
			byte data;
			public void Set(byte mask) => data |= mask;
			public readonly bool Get(byte mask = 0b0011) => (data & mask) != 0;
		}
	}
}
