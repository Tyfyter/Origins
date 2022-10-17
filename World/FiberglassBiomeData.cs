using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
using Origins.Tiles.Other;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Fiberglass_Undergrowth : ModBiome {
		public static SpawnConditionBestiaryInfoElement BestiaryBackground { get; private set; }
		public override void Load() {
			BestiaryBackground = new SpawnConditionBestiaryInfoElement("Mods.Origins.Bestiary_Biomes.Fiberglass_Underbrush", 0, "Images/MapBG1");
		}
		public override void Unload() {
			BestiaryBackground = null;
		}
		public override bool IsBiomeActive(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneFiberglass = OriginSystem.fiberglassTiles > Fiberglass_Undergrowth.NeededTiles;
		}
        public const int NeededTiles = 250;
        public const int ShaderTileCount = 75;
        public static class SpawnRates {
        }
        public static class Gen {
			public static void FiberglassStart(int i, int j) {
				for (int x = -24; x <= 24; x++) {
					for (int y = 24; y <= 24; y++) {
						SpreadFiberglass(i + x, j + y, (x * x + y * y < 12 * 12));
					}
				}
			}
			public static void SpreadFiberglass(int i, int j, bool clear, int maxRange = 32) {
				ushort tileType = (ushort)ModContent.TileType<Fiberglass_Tile>();
				ushort wallType = WallID.Glass;//(ushort)ModContent.WallType<Fiberglass_Wall>();
				Stack<(int x, int y)> tiles = new();
				tiles.Push((i, j));
				loop:
				(int x, int y) = tiles.Pop();
				if ((Math.Pow(x - i, 2) + Math.Pow(y - j, 2) < maxRange * maxRange)) {
					Tile tile = Framing.GetTileSafely(x, y);
					if (clear) {
						tile.WallType = wallType;
						if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType] && WorldGen.CanKillTile(x, y)) {
							if (tile.HasTile) {
								tiles.Push((x - 1, y));
								tiles.Push((x, y - 1));
								tiles.Push((x + 1, y));
								tiles.Push((x, y + 1));
							}
							tile.ClearTile();
						}
					} else {
						if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType] && tile.HasTile && !tile.TileIsType(tileType) && WorldGen.CanKillTile(x, y)) {
							tile.ResetToType(tileType);
							tile.WallType = wallType;
							tiles.Push((x - 1, y));
							tiles.Push((x, y - 1));
							tiles.Push((x + 1, y));
							tiles.Push((x, y + 1));
						}
					}
				}
				if (tiles.Count > 0) goto loop;
			}
		}
    }
}
