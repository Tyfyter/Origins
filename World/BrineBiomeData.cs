using AltLibrary.Common.Systems;
using Origins.Core;
using Origins.Items.Accessories;
using Origins.Items.Other.Fish;
using Origins.Items.Tools.Liquids;
using Origins.Items.Weapons.Summoner;
using Origins.Liquids;
using Origins.Tiles.Brine;
using Origins.Walls;
using Origins.Water;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Brine_Pool : ModBiome {
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconBrine";
		public override int Music => Origins.Music.BrinePool;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Brine_Pool";
		public override string MapBackground => BackgroundPath;
		public override ModWaterStyle WaterStyle => GetInstance<Brine_Water_Style>();
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => GetInstance<Placeholder_Surface_Background>();
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => null;
		public static bool forcedBiomeActive;
		public override void Load() {
			On_Player.UpdateBiomes += On_Player_UpdateBiomes;
			MonoModHooks.Add(typeof(WaterStylesLoader).GetMethod("ChooseStyle"), (orig_ChooseStyle _, WaterStylesLoader _, out int style, out SceneEffectPriority priority) => {
				SceneEffectLoader.SceneEffectInstance.PrioritizedPair waterStyle = Main.LocalPlayer.CurrentSceneEffect.waterStyle;
				style = waterStyle.value;
				priority = waterStyle.priority;
				if (priority == SceneEffectPriority.None) style = -1;
			});
		}
		delegate void orig_ChooseStyle(WaterStylesLoader self, out int style, out SceneEffectPriority priority);

		static void On_Player_UpdateBiomes(On_Player.orig_UpdateBiomes orig, Player self) {
			orig(self);
			/*if (self.CurrentSceneEffect.waterStyle.value == GetInstance<Brine_Water_Style>().Slot) {
				self.CurrentSceneEffect.waterStyle.value = WaterStyleID.Jungle;
			}*/
		}

		public override bool IsBiomeActive(Player player) {
			return OriginSystem.brineTiles > Brine_Pool.NeededTiles;
		}
		public override float GetWeight(Player player) {
			return (OriginSystem.brineTiles / (float)Brine_Pool.NeededTiles) * 0.5f;
		}
		public const int NeededTiles = 250;
		public const int ShaderTileCount = 75;
		public class DisableOtherSpawns : SpawnPool {
			public override string Name => $"{nameof(Brine_Pool)}_{base.Name}";
			public override void SetStaticDefaults() {
				Priority = SpawnPoolPriority.Environment;
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) {
				if (SpawnRates.IsInBrinePool(spawnInfo) || !spawnInfo.Player.InModBiome<Brine_Pool>()) return false;

				return Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY - 1].WallType == WallType<Baryte_Wall>()
					|| Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY).WallType == WallType<Baryte_Wall>() || Brine_Pool.forcedBiomeActive;
			}
		}
		public class SpawnRates : SpawnPool {
			public const float Carpalfish = 8000f;
			public const float Dragon = 6000f;
			public const float Creeper = 6000f;
			public const float Crab = 6000f;
			public const float A_GUN = 9000f; // yes, this is a reference
			public const float Turtle = 6000f;
			public const float Swarm = 9000f;
			public const float Snek = 9000f;
			public const float Crawdad = 9000f;
			public const float Airsnatcher = 8000f;
			public const float Dead_Guy = 1000f;
			public override string Name => $"{nameof(Brine_Pool)}_{base.Name}";
			public override void SetStaticDefaults() {
				Priority = SpawnPoolPriority.Environment;
			}
			[SuppressMessage("Style", "IDE0060:Remove unused parameter")]
			public static float EnemyRate(NPCSpawnInfo spawnInfo, float rate, bool needsMoss = false) {
				return rate;
			}

			public static bool IsInBrinePool(NPCSpawnInfo spawnInfo) {
				Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1);
				return tile.LiquidAmount >= 255 && tile.LiquidType == Brine.ID && (tile.WallType == WallType<Baryte_Wall>() || forcedBiomeActive);
			}
			public static bool IsInBrinePool(Vector2 pos) {
				Tile tile = Framing.GetTileSafely(pos);
				return tile.LiquidAmount >= 255 && tile.LiquidType == Brine.ID && (tile.WallType == WallType<Baryte_Wall>() || forcedBiomeActive);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) => IsInBrinePool(spawnInfo);
		}
		public static class Gen {
			internal delegate IEnumerable<(int min, int max, int padding)> InvalidRangeHandler(object pass, int minPriority);
			internal static InvalidRangeHandler JungleAvoider => _JungleAvoider;
			[SuppressMessage("Style", "IDE1006:Naming Styles")]
			static IEnumerable<(int min, int max, int padding)> _JungleAvoider(object pass, int minPriority) {
				int brineX = GetInstance<OriginSystem>().brineCenter.X;
				yield return (brineX - 100, brineX + 100, 50);
			}
			public static void PullTogether(ref Vector2 currentCell, ref Vector2 targetCell) {
				Vector2 diff = (targetCell - currentCell).Normalized(out float remainingDist);
				Tile currentTile = Framing.GetTileSafely(currentCell.ToPoint());
				while (!currentTile.HasTile) {
					currentCell += diff;
					currentTile = Framing.GetTileSafely(currentCell.ToPoint());
					if (--remainingDist < 0) break;
				}
				while (currentTile.HasTile) {
					currentCell -= diff;
					currentTile = Framing.GetTileSafely(currentCell.ToPoint());
					remainingDist++;
				}

				currentTile = Framing.GetTileSafely(currentCell.ToPoint());
				while (!currentTile.HasTile) {
					targetCell -= diff;
					currentTile = Framing.GetTileSafely(targetCell.ToPoint());
					if (--remainingDist < 0) break;
				}
				while (currentTile.HasTile) {
					targetCell += diff;
					currentTile = Framing.GetTileSafely(targetCell.ToPoint());
					remainingDist++;
				}
			}
			public static void BrineStart(int i, int j) {
				WorldBiomeGeneration.ChangeRange.ResetRange();
				float angle0 = genRand.NextFloat(MathHelper.TwoPi);
				float scale = 1f;
				List<Vector2> cells = [];
				HashSet<Vector2> outerCells = [];
				for (float angle1 = genRand.NextFloat(6f, 8f); angle1 > 0; angle1 -= genRand.NextFloat(0.5f, 0.7f)) {
					float totalAngle = angle0 + angle1;
					float length = genRand.NextFloat(20f, 40f) * scale;
					genCave:
					Vector2 offset = OriginExtensions.Vec2FromPolar(totalAngle, length * scale);
					Vector2 pos = new(i + offset.X, j + offset.Y);
					const float intolerance = 16;
					if (!cells.Any((v) => (v - pos).LengthSquared() < intolerance * intolerance)) {
						SmallCave(
							pos.X, pos.Y,
							genRand.NextFloat(1.125f, 1.225f) * scale,
							OriginExtensions.Vec2FromPolar(totalAngle, MathF.Pow(genRand.NextFloat(0.25f, 1f), 1.5f))
						);
						cells.Add(pos);
					}


					if (length < 50 * scale) {
						length += 24 * scale;
						totalAngle += (genRand.NextBool() ? 1 : -1) * genRand.NextFloat(0.6f, 1.2f);
						goto genCave;
					}
					outerCells.Add(pos);
				}
				SmallCave(
					i, j,
					genRand.NextFloat(1.4f, 1.6f) * scale,
					OriginExtensions.Vec2FromPolar(genRand.NextFloat(MathHelper.TwoPi), MathF.Pow(genRand.NextFloat(0.25f, 0.7f), 1.5f) * 1.5f)
				);
				ushort stoneID = (ushort)TileType<Baryte>();
				ushort stoneWallID = (ushort)WallType<Baryte_Wall>();
				/*Tile tile;
				for (int x = (int)MathF.Floor(i - 55); x < (int)MathF.Ceiling(i + 55); x++) {
					for (int y = (int)MathF.Ceiling(j - 55); y < (int)MathF.Floor(j + 55); y++) {
						tile = Framing.GetTileSafely(x, y);
						if (tile.HasTile && !WorldGen.CanKillTile(x, y)) continue;
						Vector2 diff = new(y - j, x - i);
						float distSQ = diff.LengthSquared() * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1);
						if (distSQ > 55 * 55) {
							continue;
						}
						if (tile.WallType != stoneWallID) {
							tile.ResetToType(stoneID);
						}
						tile.WallType = stoneWallID;
					}
				}*/
				ushort mossID = (ushort)TileType<Peat_Moss>();
				int cellCount = cells.Count;
				bool[] canBeClearedDuringOreRunner = TileID.Sets.CanBeClearedDuringOreRunner;
				try {
					void DoOre(ushort oreID, bool[] validOreTiles, double strength = 7, int steps = 5) {
						List<Vector2> cellsForOre = cells.ToList();
						TileID.Sets.CanBeClearedDuringOreRunner = validOreTiles;
						for (int i0 = genRand.Next((int)(cellCount * 0.8f), cellCount); i0 > 0; i0--) {
							float oreAngle;
							Vector2 pos = genRand.Next(cellsForOre);
							cellsForOre.Remove(pos);
							if (oreID != stoneID && outerCells.Contains(pos)) {
								float centerAngle = (new Vector2(i, j) - pos).ToRotation();
								oreAngle = genRand.NextFloat(centerAngle - MathHelper.PiOver2, centerAngle + MathHelper.PiOver2);
							} else {
								oreAngle = genRand.NextFloat(MathHelper.TwoPi);
							}
							Vector2 step = OriginExtensions.Vec2FromPolar(oreAngle, 1);
							Tile currentTile = Framing.GetTileSafely(pos.ToPoint());
							while (!currentTile.HasTile) {
								pos += step;
								currentTile = Framing.GetTileSafely(pos.ToPoint());
							}
							int stepCount = 0;
							Vector2 endPos = pos;
							while (currentTile.HasTile && stepCount < 5) {
								endPos += step;
								stepCount++;
								currentTile = Framing.GetTileSafely(pos.ToPoint());
							}
							pos = (pos + endPos) / 2;
							if (currentTile.TileType == mossID || currentTile.TileType == TileID.Mud) {
								//TODO directional ore runner that returns ore count
								//TODO do it again if low total ore in cell and random chance
								OreRunner((int)pos.X, (int)pos.Y, strength, steps, oreID);
							}
						}
					}
					bool[] validOreTiles = TileID.Sets.Factory.CreateBoolSet(stoneID, TileID.Mud, mossID);
					DoOre((ushort)TileType<Eitrite_Ore>(), validOreTiles);
					validOreTiles[stoneID] = false;
					DoOre(stoneID, validOreTiles, steps: 3);
				} finally {
					TileID.Sets.CanBeClearedDuringOreRunner = canBeClearedDuringOreRunner;
				}
				List<Vector2> cellsForConnections = cells.ToList();
				HashSet<UnorderedTuple<Vector2>> connections = new();
				using SetOverride<bool> _ = new(TileID.Sets.CanBeClearedDuringGeneration, stoneID, true);
				for (int i0 = genRand.Next((int)(cellCount * 0.4f), (int)(cellCount * 0.8f)); i0 > 0; i0--) {
					Vector2 currentCell = genRand.Next(cellsForConnections);
					cellsForConnections.Remove(currentCell);
					Vector2 targetCell = genRand.Next(cellsForConnections.Where(v0 => {
						Vector2 potentialDiff = v0 - currentCell;
						float potentialLength = potentialDiff.Length();
						potentialDiff /= potentialLength;
						return !cellsForConnections.Any(
							v1 => {
								if (v1 == v0) return false;
								Vector2 otherDiff = v1 - currentCell;
								float otherLength = otherDiff.Length();
								otherDiff /= otherLength;
								return potentialLength > otherLength && Vector2.Dot(potentialDiff, otherDiff) > 0.15f;
							}
						);
					}).ToList());
					connections.Add((currentCell, targetCell));

					PullTogether(ref currentCell, ref targetCell);
					Vector2 diff = (targetCell - currentCell).Normalized(out float dist);
					Vector2 offshoot = diff.RotatedByRandom(0.1f);
					GenRunners.WalledVeinRunner(
						(int)currentCell.X, (int)currentCell.Y,
						genRand.NextFloat(1.8f, 3),
						offshoot,
						dist / Vector2.Dot(diff, offshoot),
						stoneID,
						1,
						wallType: stoneWallID
					);
				}
				{
					List<Vector2> validSurfaceCells = [];
					if (WorldGen.remixWorldGen) {
						float dist = 0;
						for (int index = 0; index < cells.Count; index++) {
							Vector2 cell = cells[index];
							float newDist = Math.Abs(cell.X - i);
							if (newDist > dist) {
								dist = newDist;
								validSurfaceCells.Clear();
							}
							validSurfaceCells.Add(cell);
						}
					} else {
						for (int index = 0; index < cells.Count; index++) {
							Vector2 cell = cells[index];
							if (cell.Y > j) continue;
							bool foundAnyCompetition = false;
							for (int index2 = 0; index2 < validSurfaceCells.Count; index2++) {
								Vector2 otherCell = validSurfaceCells[index2];
								if (Math.Abs(otherCell.X - cell.X) < 35) {
									if (otherCell.Y > cell.Y) {
										if (foundAnyCompetition) {
											validSurfaceCells.RemoveAt(index2--);
										} else {
											validSurfaceCells[index2] = cell;
										}
										foundAnyCompetition = true;
									} else {
										foundAnyCompetition = true;
										break;
									}
								}
							}
							if (!foundAnyCompetition) validSurfaceCells.Add(cell);
						}
					}
					Vector2 surfaceConnection = genRand.Next(validSurfaceCells);
					// make sure the surface cell has 
					{
						bool needsNewConnection = true;
						List<Vector2> validOthers = cells.Where(v0 => {
							if (v0 == surfaceConnection || connections.Contains((surfaceConnection, v0))) return needsNewConnection = false;
							Vector2 potentialDiff = (v0 - surfaceConnection).Normalized(out float potentialLength);
							return !cells.Any(
								v1 => {
									if (v1 == v0) return false;
									Vector2 otherDiff = v1 - surfaceConnection;
									float otherLength = otherDiff.Length();
									otherDiff /= otherLength;
									float dot = Vector2.Dot(potentialDiff, otherDiff);
									return potentialLength > otherLength && dot > 0.15f;
								}
							);
						}).ToList();
						if (needsNewConnection && validOthers.Count <= 0) {
							validOthers.Add(cells.Except([surfaceConnection]).MinBy(o => surfaceConnection.DistanceSQ(o)));
						}
						if (validOthers.Count > 0) {
							Vector2 currentCell = surfaceConnection;
							Vector2 targetCell = genRand.Next(validOthers);

							PullTogether(ref currentCell, ref targetCell);
							Vector2 diff = targetCell - currentCell;
							Vector2 offshoot = diff.RotatedByRandom(0.1f);
							GenRunners.WalledVeinRunner(
								(int)currentCell.X, (int)currentCell.Y,
								genRand.NextFloat(1.8f, 3),
								offshoot,
								diff.Length() / Vector2.Dot(diff, offshoot),
								stoneID,
								1,
								wallType: stoneWallID
							);
						} else {
							Origins.instance.Logger.Warn("No brine pool surface connection generated, connection may have already been generated");
						}
					}
					bool[] validTiles = TileID.Sets.CanBeClearedDuringGeneration.ToArray();
					validTiles[stoneID] = true;
					validTiles[mossID] = true;
					validTiles[TileID.Mud] = true;
					while (!Framing.GetTileSafely(surfaceConnection.ToPoint()).HasTile) {
						surfaceConnection.Y--;
					}
					retryDir:
					Vector2 direction = -Vector2.UnitY.RotatedByRandom(0.15f);
					if (WorldGen.remixWorldGen) {
						direction.X = float.CopySign(direction.Y * genRand.NextFloat(1f, 1.25f), surfaceConnection.X - i);
						direction.Normalize();
					}
					if (direction.HasNaNs()) goto retryDir;
					GenRunners.OpeningRunner(
						(int)surfaceConnection.X, (int)surfaceConnection.Y,
						genRand.NextFloat(4, 6),
						genRand.NextFloat(0.95f, 1.2f),
						direction,
						75,
						validTiles
					);
				}
				ushort rivenAltar = (ushort)TileType<Hydrothermal_Vent>();
				Rectangle range = WorldBiomeGeneration.ChangeRange.GetRange();
				for (int k = genRand.Next(40, 60); k > 0;) {
					int posX = genRand.Next(range.Left, range.Right);
					int posY = genRand.Next(range.Top, range.Bottom);
					if (Framing.GetTileSafely(posX, posY).WallType == stoneWallID && TileExtenstions.CanActuallyPlace(posX, posY, rivenAltar, 0, 0, out TileObject objectData, false, checkStay: true)) {
						TileObject.Place(objectData);
						k--;
					}
				}
				int vineTries = 0;
				int brineglowTile = TileType<Brineglow>();
				int vineTile = TileType<Underwater_Vine>();
				int vineCount = 0;
				for (int k = genRand.Next(400, 600); k > 0;) {
					int posX = genRand.Next(range.Left, range.Right);
					int posY = genRand.Next(range.Top, range.Bottom);
					int length = 0;
					bool isVine = false;
					Tile currentTile;
					for (; (currentTile = Framing.GetTileSafely(posX, posY)).HasTile; posY++) if (currentTile.TileType == brineglowTile || currentTile.TileType == vineTile) isVine = true;
					if (!isVine) {
						int currentVineTile = WorldGen.genRand.NextBool(3) ? brineglowTile : vineTile;
						for (; TileObject.CanPlace(posX, posY, currentVineTile, 0, 0, out TileObject objectData, false, checkStay: true); posY++) {
							objectData.style = 0;
							objectData.alternate = 0;
							objectData.random = 0;
							TileObject.Place(objectData);
							if (Framing.GetTileSafely(posX, posY).TileType == currentVineTile) Framing.GetTileSafely(posX, posY).TileFrameX = -1;
							length++;
							if (genRand.NextBool(12 - length)) break;
						}
					}
					if (length > 0 || ++vineTries > 1000) k--;
					if (length > 0) vineCount++;
				}
				ushort replacementWallID = (ushort)WallType<Replacement_Wall>();
				for (int x = range.Left; x < range.Right; x++) {
					for (int y = range.Top; y < range.Bottom; y++) {
						Tile tile = Main.tile[x, y];
						if (tile.WallType == replacementWallID) {
							tile.WallType = (ushort)tile.Get<TemporaryWallData>().value;
						}
						if (!tile.HasTile) {
							Tile tileAbove = Main.tile[x, y - 1];
							if (tileAbove.HasTile && (TileID.Sets.PreventsTileRemovalIfOnTopOfIt[tileAbove.TileType] || TileExtenstions.IsBrokenBottomAnchor(x, y))) {
								tile.HasTile = true;
								if (!Main.tileContainer[tile.TileType]) tile.TileType = mossID;
							}
						}
					}
				}
				OriginSystem.Instance.brinePoolRange = range;
				GenVars.structures?.AddProtectedStructure(range, 6);
			}
			public static void SmallCave(float i, float j, float sizeMult = 1f, Vector2 stretch = default) {
				ushort stoneID = (ushort)TileType<Baryte>();
				ushort mossID = (ushort)TileType<Peat_Moss>();
				ushort stoneWallID = (ushort)WallType<Baryte_Wall>();
				ushort replacementWallID = (ushort)WallType<Replacement_Wall>();
				float stretchScale = stretch.Length();
				Vector2 stretchNorm = stretch / stretchScale;
				float totalSize = 20 * sizeMult * (stretchScale + 1);
				Tile tile;
				for (int x = (int)Math.Floor(i - totalSize); x < (int)Math.Ceiling(i + totalSize); x++) {
					for (int y = (int)Math.Ceiling(j - totalSize); y < (int)Math.Floor(j + totalSize); y++) {
						tile = Framing.GetTileSafely(x, y);
						Vector2 diff = new(y - j, x - i);
						float distSQ = diff.LengthSquared() * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1);
						float dist = (float)Math.Sqrt(distSQ);
						dist *= Math.Abs(Vector2.Dot(diff.SafeNormalize(default), stretchNorm) * stretchScale) + 1;
						if (dist > 20 * sizeMult) {
							continue;
						}
						if ((!Main.tileCut[tile.TileType] && tile.TileType is not TileID.Trees or TileID.SmallPiles) && (Main.tileFrameImportant[tile.TileType] || Main.tileSolidTop[tile.TileType] || !Main.tileSolid[tile.TileType])) {
							tile.WallType = stoneWallID;
							switch (tile.TileType) {
								case TileID.SmallPiles or TileID.LargePiles or TileID.LargePiles2:
								break;
								default:
								if (TileID.Sets.Boulders[tile.TileType]) {
									break;
								}
								continue;
							}
							tile.HasTile = false;
							if (dist <= 19 * sizeMult) tile.WallType = stoneWallID;
							else {
								tile.Get<TemporaryWallData>().value = tile.WallType;
								tile.WallType = replacementWallID;
							}
							continue;
						}
						if (tile.WallType != stoneWallID && tile.WallType != replacementWallID) {
							tile.ResetToType(stoneID);
						}
						if (dist <= 19 * sizeMult) tile.WallType = stoneWallID;
						else {
							tile.Get<TemporaryWallData>().value = tile.WallType;
							tile.WallType = replacementWallID;
						}
						if (dist < 20 * sizeMult - 10f) {
							if (WorldGen.CanKillTile(x, y)) tile.HasTile = false;
						} else if (dist < 20 * sizeMult - 6f) {
							tile.ResetToType(mossID);
						} /*else if (dist < 20 * sizeMult - 7f) {
							tile.ResetToType(genRand.NextBool(5) ? mossID : TileID.Mud);
						}*/
						tile.LiquidType = Brine.ID;
						tile.LiquidAmount = 255;
						WorldBiomeGeneration.ChangeRange.AddChangeToRange(x, y);
					}
				}
			}
			public static Point BrineStart_Old(int i, int j, float sizeMult = 1f) {
				ushort stoneID = (ushort)TileType<Baryte>();
				ushort stoneWallID = WallID.BlueDungeonSlab;//(ushort)WallType<Riven_Flesh_Wall>();
				int i2 = i + (int)(genRand.Next(-22, 22) * sizeMult);
				int j2 = j + (int)(44 * sizeMult);
				for (int x = i2 - (int)(66 * sizeMult + 10); x < i2 + (int)(66 * sizeMult + 10); x++) {
					for (int y = j2 + (int)(56 * sizeMult + 8); y >= j2 - (int)(56 * sizeMult + 8); y--) {
						float j3 = (Math.Min(j2, y) + j2 * 2) / 3f;
						float sq = Math.Max(Math.Abs(y - j3) * 1.5f, Math.Abs(x - i2));
						float pyth = (((y - j3) * (y - j3) * 1.5f) + (x - i2) * (x - i2));
						//define the distance between the point and center as a combination of Euclidian distance (dist = sqrt(xdist² + ydist²)) and Chebyshev distance (dist = max(xdist, ydist))
						float diff = (float)Math.Sqrt((sq * sq + (pyth * 3)) * 0.25f * (GenRunners.GetWallDistOffset(x) * 0.0316076058772687986171132238548f + 1));
						if (diff > 70 * sizeMult) {
							continue;
						}

						switch (Main.tile[x, y].HasTile ? Main.tile[x, y].TileType : -1) {
							case TileID.IridescentBrick:
							case TileID.TinBrick:
							case TileID.GoldBrick:
							case TileID.Mudstone:
							if (Main.tileContainer[Main.tile[x, y - 1].TileType] || genRand.Next(5) > 0) {
								break;
							}
							goto default;
							case TileID.LivingMahogany:
							Main.tile[x, y].TileType = TileID.Ash;
							break;
							default:
							if (Main.tileContainer[Main.tile[x, y].TileType]) {
								break;
							}
							OriginSystem.RemoveTree(x, y - 1);
							Main.tile[x, y].ResetToType(stoneID);
							if (diff < 70 * sizeMult - 10 || ((y - j) * (y - j)) + ((x - i) * (x - i) * 0.5f) < 700 * sizeMult * sizeMult) {//(x - i) * 
								if (Main.tileContainer[Main.tile[x, y - 1].TileType]) {
									break;
								}
								Main.tile[x, y].SetActive(false);
								//if (y > j2 - (sizeMult * 32)) {
								Main.tile[x, y].LiquidAmount = 255;
								//}
							}
							break;
						}
						switch (Main.tile[x, y].WallType) {
							case WallID.IridescentBrick:
							case WallID.TinBrick:
							case WallID.GoldBrick:
							case WallID.MudstoneBrick:
							if (genRand.NextBool(5)) {
								goto default;
							}
							break;
							default:
							Main.tile[x, y].WallType = stoneWallID;
							break;
						}
					}
				}
				int c = 0;
				float size = 70;
				int wallSize = 10;
				Vector2 topLeft = new(i2, (float)GenVars.worldSurfaceHigh);
				Vector2 topRight = new(i2, (float)GenVars.worldSurfaceHigh);
				int minX = int.MaxValue;
				int maxX = int.MinValue;
				for (int y = j2 - (int)(50 * sizeMult + 8); y > GenVars.worldSurfaceLow; y--) {
					c++;
					int changed = 0;
					for (int x = i2 - (int)(66 * sizeMult + 10); x < i2 + (int)(66 * sizeMult + 10); x++) {
						float j3 = (Math.Min(j2 - c, y) + (j2 - c) * 2) / 3f;
						float sq = Math.Max(Math.Abs(y - j3) * 1.5f, Math.Abs(x - i2));
						float pyth = ((y - j3) * (y - j3) * 1.5f) + (x - i2) * (x - i2);
						float diff = (float)Math.Sqrt((sq * sq + (pyth * 3)) * 0.25f * (GenRunners.GetWallDistOffset((x > i2 ? c : -c)) * 0.0105358686257562662057044079516f + 1));
						if (diff > size * sizeMult) {
							continue;
						}
						bool change = false;
						switch (Main.tile[x, y].TileType) {
							case TileID.IridescentBrick:
							case TileID.TinBrick:
							case TileID.GoldBrick:
							case TileID.Mudstone:
							if (Main.tileContainer[Main.tile[x, y - 1].TileType] || genRand.Next(5) > 0) {
								break;
							}
							goto default;
							case TileID.LivingMahogany:
							Main.tile[x, y].TileType = TileID.Ash;
							break;
							default:
							if (Main.tileContainer[Main.tile[x, y].TileType]) {
								break;
							}
							if (y > GenVars.worldSurfaceHigh || (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])) {
								Main.tile[x, y].ResetToType(stoneID);
								change = true;
							}
							if (diff < size * sizeMult - wallSize) {//(x - i) * 
								if (Main.tileContainer[Main.tile[x, y - 1].TileType]) {
									break;
								}
								if (Main.tile[x, y].HasTile) change = true;
								Main.tile[x, y].SetActive(false);
								OriginSystem.RemoveTree(x, y - 1);
								if (y > GenVars.worldSurfaceHigh) {
									Main.tile[x, y].LiquidAmount = 255;
								}
							}
							if (y < GenVars.worldSurfaceHigh && Main.tile[x, y].HasTile && change) {
								if (x > i2) {//right side
									if (x > maxX) {
										maxX = x;
									}
									if (y <= topRight.Y) {
										topRight = new Vector2(x, y);
									}
								} else {//left side
									if (x < minX) {
										minX = x;
									}
									if (y < topLeft.Y) {
										topLeft = new Vector2(x, y);
									}
								}
							}
							break;
						}
						switch (Main.tile[x, y].WallType) {
							case WallID.IridescentBrick:
							case WallID.TinBrick:
							case WallID.GoldBrick:
							case WallID.MudstoneBrick:
							if (genRand.NextBool(5)) {
								goto default;
							}
							break;
							default:
							if (y > GenVars.worldSurfaceHigh) {
								Main.tile[x, y].WallType = stoneWallID;
							}
							break;
						}
						if (change) {
							changed++;
						}
					}
					if (y < GenVars.worldSurfaceHigh) {
						size -= 0.03f;
					}
					if (changed < 23 * sizeMult + 10) {
						break;
					}
				}
				int top = (int)Math.Max(topLeft.Y, topRight.Y);
				float slope = (topLeft.Y - topRight.Y) / (topRight.X - topLeft.X);
				float minY;
				int prog = 0;
				for (int x = minX; x < maxX; x++) {
					minY = top - slope * prog + GenRunners.GetWallDistOffset(x + top) * 0.4f;
					if (x >= topLeft.X && x <= topRight.X) {
						prog++;
					}
					for (int y = (int)(GenVars.worldSurfaceHigh + 1); y >= minY; y--) {
						Main.tile[x, y].WallType = stoneWallID;
					}
				}

				return new Point(i2, j2);
			}
		}
		public class Placeholder_Surface_Background : ModSurfaceBackgroundStyle {
			public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
				b = 1660;
				return Main.jungleBG[2];
			}
			public override int ChooseMiddleTexture() {
				return Main.jungleBG[1];
			}
			public override int ChooseFarTexture() {
				return Main.jungleBG[0];
			}
			public override void ModifyFarFades(float[] fades, float transitionSpeed) {

			}
		}
		public class Brine_Pool_Fishing_Loot : FishingLootPool {
			public override bool IsActive(Player player, FishingAttempt attempt) => player.InModBiome<Brine_Pool>() || attempt.BobberInLiquid<Brine>();
			public override void SetStaticDefaults() {
				Crate.AddRange([
					FishingCatch.Item(GetInstance<Brine_Crates>().GetItem(HardmodeVariant.Normal).Type, (player, attempt) => !Main.hardMode && ShouldDropBiomeCrate(player, attempt) && player.InModBiome<Brine_Pool>()),
					FishingCatch.Item(GetInstance<Brine_Crates>().GetItem(HardmodeVariant.Hardmode).Type, (player, attempt) => Main.hardMode && ShouldDropBiomeCrate(player, attempt) && player.InModBiome<Brine_Pool>())
				]);
				Legendary.AddRange([
					FishingCatch.Item(ItemType<Brine_Bottomless_Bucket>()),
					FishingCatch.Item(ItemType<Brine_Sponge>())
				]);
				Rare.Add(FishingCatch.Item(ItemType<Huff_Puffer_Bait>(), (player, attempt) => player.InModBiome<Brine_Pool>()));
				Uncommon.AddRange([
					FishingCatch.Item(ItemType<Bobbit_Worm>(), (player, attempt) => player.InModBiome<Brine_Pool>() && attempt.questFish == ItemType<Bobbit_Worm>()),
					FishingCatch.Item(ItemType<Mithrafin>()),
					FishingCatch.Item(ItemType<Toadfish>(), weight: 9),
					new FallthroughFishingCatch((player, _) => !player.InModBiome<Brine_Pool>(), 40)
				]);
			}
		}
	}
	public class Brine_Pool_Water_Control : ModSceneEffect {
		public override ModWaterStyle WaterStyle => GetInstance<Brine_Water_Style>();
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsSceneEffectActive(Player player) => OriginSystem.brineTiles > Brine_Pool.NeededTiles;
		public override float GetWeight(Player player) => 1f;
	}
}
