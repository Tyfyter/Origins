using Origins.Backgrounds;
using Origins.NPCs.Fiberglass;
using Origins.Tiles.Other;
using Origins.Walls;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
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
		public override void SpecialVisuals(Player player, bool isActive) {
			player.ManageSpecialBiomeVisuals("Origins:ZoneFiberglassUndergrowth", Fiberglass_Wall.AnyWallsVisible, player.Center);
		}
		public const int NeededTiles = 1000;
		public const int ShaderTileCount = 75;
		public class DisableOtherSpawns : SpawnPool {
			public override string Name => $"{nameof(Fiberglass_Undergrowth)}_{base.Name}";
			public override bool IsActive(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome<Fiberglass_Undergrowth>() && spawnInfo.SpawnTileType != ModContent.TileType<Fiberglass_Tile>();
		}
		public class SpawnRates : SpawnPool {
			public override string Name => $"{nameof(Fiberglass_Undergrowth)}_{base.Name}";
			public override void SetStaticDefaults() {
				AddSpawn<Enchanted_Fiberglass_Sword>(1);
				AddSpawn<Enchanted_Fiberglass_Bow>(1);
				AddSpawn<Enchanted_Fiberglass_Pistol>(1);
				AddSpawn<Enchanted_Fiberglass_Cannon>(1);
				Enchanted_Fiberglass_Slime.AddSpawns(this);
				AddSpawn<Fiberglass_Weaver>(info => NPC.downedBoss3 && (info.Player.HasBuff(BuffID.Poisoned) || info.Player.HasBuff(BuffID.Venom)) ? 0.025f : 0);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) => spawnInfo.SpawnTileType == ModContent.TileType<Fiberglass_Tile>();
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
				ushort tileType = (ushort)ModContent.TileType<Fiberglass_Tile>();
				//SpreadFiberglass(i + x, j + y, (distSq < 20 * 20), 12);
				Tile tile = Framing.GetTileSafely(i + x, j + y);
				SpreadFiberglassWalls(i + x, j + y, (distSq < 18 * 18) || (tile.HasTile && tile.TileType != tileType && WorldGen.genRand.NextBool(8)), 14);
			}
			public static void SpreadFiberglass(int i, int j, bool clear, int maxRange = 20) {
				ushort tileType = (ushort)ModContent.TileType<Fiberglass_Tile>();
				Stack<(int x, int y)> tiles = new();
				HashSet<(int x, int y)> alreadyProcessed = [];
				tiles.Push((i, j));
				while (tiles.Count > 0) {
					(int x, int y) = tiles.Pop();
					Tile tile = Framing.GetTileSafely(x, y);
					if ((TileID.Sets.IsATreeTrunk[tile.TileType] || (Math.Abs(x - i) * 2.1f + Math.Pow(y - j, 2) < maxRange * maxRange)) && alreadyProcessed.Add((i, j))) {
						if (clear) {
							if (OriginExtensions.IsTileReplacable(x, y)) {
								if (tile.HasTile) {
									tiles.Push((x - 1, y));
									tiles.Push((x, y - 1));
									tiles.Push((x + 1, y));
									tiles.Push((x, y + 1));
								}
								if (!Framing.GetTileSafely(x, y - 1).TileIsType(TileID.Heart)) tile.ClearTile();
								else tile.ResetToType(tileType);
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
					double distSq = Math.Abs(x - i) * 2f + Math.Pow(y - j, 2);
					if ((distSq < maxRange * maxRange)) {
						Tile tile = Framing.GetTileSafely(x, y);
						if (force) {
							tile.WallType = wallType;
							if (distSq < Math.Pow(maxRange / 2, 2) || !WorldGen.genRand.NextBool(2)) {
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
