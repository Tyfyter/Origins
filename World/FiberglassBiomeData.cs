using Origins.Tiles.Other;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.World.BiomeData {
	public class Fiberglass_Undergrowth : ModBiome {
		public override int Music => Origins.Music.Fiberglass;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconFiberglass";
		public override string BackgroundPath => "Origins/UI/MapBGs/Fiberglass_Caverns";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneFiberglass = OriginSystem.fiberglassTiles > Fiberglass_Undergrowth.NeededTiles;
		}
		public const int NeededTiles = 1000;
		public const int ShaderTileCount = 75;
		public static class SpawnRates {
			public const float Sword = 1;
			public const float Bow = 1;
			public const float Gun = 1;
			public const float Spider = 0.025f;
		}
		public static class Gen {
			public static void FiberglassStart(int i, int j) {
				for (int x = 0; x <= 32; x++) {
					for (int y = 0; y <= 32; y++) {
						TrySpread(i, j, x, y);
						TrySpread(i, j, -x, y);
						TrySpread(i, j, x, -y);
						TrySpread(i, j, -x, -y);
					}
				}
			}
			public static void TrySpread(int i, int j, int x, int y) {
				float distSq = (x * x + y * y) * (GenRunners.GetWallDistOffset((float)Math.Atan2(y, x) * 4 + x + y) * 0.04f + 1.5f);
				//SpreadFiberglass(i + x, j + y, (distSq < 20 * 20), 12);
				SpreadFiberglassWalls(i + x, j + y, (distSq < 18 * 18), 14);
			}
			public static void SpreadFiberglass(int i, int j, bool clear, int maxRange = 20) {
				ushort tileType = (ushort)ModContent.TileType<Fiberglass_Tile>();
				Stack<(int x, int y)> tiles = new();
				tiles.Push((i, j));
				while (tiles.Count > 0) {
					(int x, int y) = tiles.Pop();
					Tile tile = Framing.GetTileSafely(x, y);
					if (TileID.Sets.IsATreeTrunk[tile.TileType] || (Math.Pow(x - i, 2) + Math.Pow(y - j, 2) < maxRange * maxRange)) {
						if (clear) {
							if (OriginExtensions.IsTileReplacable(x, y) && !tile.TileIsType(tileType)) {
								if (tile.HasTile) {
									tiles.Push((x - 1, y));
									tiles.Push((x, y - 1));
									tiles.Push((x + 1, y));
									tiles.Push((x, y + 1));
								}
								tile.ClearTile();
							}
						} else {
							if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType] && tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !tile.TileIsType(tileType) && WorldGen.CanKillTile(x, y)) {
								tile.ResetToType(tileType);
								tiles.Push((x - 1, y));
								tiles.Push((x, y - 1));
								tiles.Push((x + 1, y));
								tiles.Push((x, y + 1));
							}
						}
					}
				}
			}
			public static void SpreadFiberglassWalls(int i, int j, bool force, int maxRange = 28) {
				ushort wallType = (ushort)ModContent.WallType<Fiberglass_Wall>();
				Stack<(int x, int y)> tiles = new();
				tiles.Push((i, j));
				while (tiles.Count > 0) {
					(int x, int y) = tiles.Pop();
					double distSq = Math.Pow(x - i, 2) + Math.Pow(y - j, 2);
					if ((distSq < maxRange * maxRange)) {
						Tile tile = Framing.GetTileSafely(x, y);
						if (force) {
							tile.WallType = wallType;
							if (distSq < Math.Pow(maxRange / 2, 2) || !WorldGen.genRand.NextBool(16)) {
								SpreadFiberglass(x, y, true, 8);
							} else {
								SpreadFiberglass(x, y, false, 2);
							}
						} else {
							if (tile.WallType != WallID.None && tile.WallType != wallType) {
								tile.WallType = wallType;
								tiles.Push((x - 1, y));
								tiles.Push((x, y - 1));
								tiles.Push((x + 1, y));
								tiles.Push((x, y + 1));
								SpreadFiberglass(x, y, false, 12);
							}
						}
					}
				}
			}
		}
	}
}
