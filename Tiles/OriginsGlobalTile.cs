﻿using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles {
	public class OriginsGlobalTile : GlobalTile {
		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
			if (Main.tile[i, j - 1].TileType == Defiled_Altar.ID && type != Defiled_Altar.ID) return false;
			return true;
		}
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			/*if () {

			}*/
		}
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
			switch (type) {
				case TileID.Plants:
				case TileID.CorruptPlants:
				case TileID.CrimsonPlants:
				case TileID.HallowedPlants:
				ConvertPlantsByAnchor(ref Main.tile[i, j].TileType, Main.tile[i, j + 1].TileType);
				return true;
			}
			if (type == ModContent.TileType<Defiled_Foliage>()) {
				ConvertPlantsByAnchor(ref Main.tile[i, j].TileType, Main.tile[i, j + 1].TileType);
			}
			return true;
		}
		public static void ConvertPlantsByAnchor(ref ushort plant, ushort anchor) {
			switch (anchor) {
				case TileID.Grass:
				plant = TileID.Plants;
				return;
				case TileID.CorruptGrass:
				plant = TileID.CorruptPlants;
				return;
				case TileID.CrimsonGrass:
				plant = TileID.CrimsonPlants;
				return;
				case TileID.HallowedGrass:
				plant = TileID.HallowedPlants;
				return;
			}
			if (anchor == ModContent.TileType<Defiled_Grass>()) {
				plant = (ushort)ModContent.TileType<Defiled_Foliage>();
			}
		}
	}
}
