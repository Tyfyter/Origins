using Microsoft.Xna.Framework;
using Origins.Backgrounds;
using Origins.Tiles.Brine;
using Origins.Walls;
using Origins.Water;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Brine_Pool : ModBiome {
		public static SpawnConditionBestiaryInfoElement BestiaryBackground { get; private set; }
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Brine_Water_Style>();
		public override void Load() {
			BestiaryBackground = new SpawnConditionBestiaryInfoElement("Mods.Origins.Bestiary_Biomes.Brine_Pool", 0, "Images/MapBG1");
		}
		public override void Unload() {
			BestiaryBackground = null;
		}
		public override bool IsBiomeActive(Player player) {
			return OriginSystem.brineTiles > Brine_Pool.NeededTiles;
		}
		public const int NeededTiles = 250;
		public const int ShaderTileCount = 75;
		public static class SpawnRates {
		}
		public static class Gen {
			static int minGenX, maxGenX, minGenY, maxGenY;
			public static void BrineStart(int i, int j) {
				minGenX = int.MaxValue;
				maxGenX = int.MinValue;
				minGenY = int.MaxValue;
				maxGenY = int.MinValue;
				float angle0 = genRand.NextFloat(MathHelper.TwoPi);
				float scale = 1f;
				List<Vector2> cells = new();
				HashSet<Vector2> outerCells = new();
				for (float angle1 = genRand.NextFloat(6f, 8f); angle1 > 0; angle1 -= genRand.NextFloat(0.5f, 0.7f)) {
					float totalAngle = angle0 + angle1;
					float length = genRand.NextFloat(24f, 48f) * scale;
					genCave:
					Vector2 offset = OriginExtensions.Vec2FromPolar(totalAngle, length * scale);
					Vector2 pos = new(i + offset.X, j + offset.Y);
					const float intolerance = 16;
					if (!cells.Any((v) => (v - pos).LengthSquared() < intolerance * intolerance)) {
						SmallCave(
							pos.X, pos.Y,
							genRand.NextFloat(1.1f, 1.2f) * scale,
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
				ushort stoneID = (ushort)ModContent.TileType<Sulphur_Stone>();
				ushort stoneWallID = (ushort)ModContent.WallType<Sulphur_Stone_Wall>();
				Tile tile;
				for (int x = (int)MathF.Floor(i - 55); x < (int)MathF.Ceiling(i + 55); x++) {
					for (int y = (int)MathF.Ceiling(j - 55); y < (int)MathF.Floor(j + 55); y++) {
						tile = Framing.GetTileSafely(x, y);
						if (tile.HasTile && !WorldGen.CanKillTile(x, y)) continue;
						Vector2 diff = new(y - j, x - i);
						float distSQ = diff.LengthSquared() * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1);
						float dist = (float)Math.Sqrt(distSQ);
						if (dist > 55) {
							continue;
						}
						if (tile.WallType != stoneWallID) {
							tile.ResetToType(stoneID);
						}
						tile.WallType = stoneWallID;
					}
				}
				ushort mossID = (ushort)ModContent.TileType<Peat_Moss_Tile>();
				ushort oreID = (ushort)ModContent.TileType<Eitrite_Ore>();
				int cellCount = cells.Count;
				List<Vector2> cellsForOre = cells.ToList();
				bool[] validOreTiles = TileID.Sets.Factory.CreateBoolSet(stoneID, TileID.Mud, mossID);
				bool[] canBeClearedDuringOreRunner = TileID.Sets.CanBeClearedDuringOreRunner; 
				try {
					TileID.Sets.CanBeClearedDuringOreRunner = validOreTiles;
					for (int i0 = genRand.Next((int)(cellCount * 0.8f), cellCount); i0 > 0; i0--) {
						float oreAngle;
						Vector2 pos = genRand.Next(cellsForOre);
						cellsForOre.Remove(pos);
						if (outerCells.Contains(pos)) {
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
							OreRunner((int)pos.X, (int)pos.Y, 4, 4, oreID);
						}
					}
				} finally {
					TileID.Sets.CanBeClearedDuringOreRunner = canBeClearedDuringOreRunner;
				}
				List<Vector2> cellsForConnections = cells.ToList();
				HashSet<UnorderedTuple<Vector2>> connections = new();
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
					Vector2 diff = targetCell - currentCell;
					float length = diff.Length();
					diff /= length;
					Vector2 pos = currentCell;
					Tile currentTile = Framing.GetTileSafely(pos.ToPoint());
					while (!currentTile.HasTile) {
						pos += diff;
						currentTile = Framing.GetTileSafely(pos.ToPoint());
					}
					int wallThickness = 0;
					while (currentTile.HasTile) {
						pos += diff;
						currentTile = Framing.GetTileSafely(pos.ToPoint());
						wallThickness++;
					}
					connections.Add((currentCell, targetCell));
					GenRunners.WalledVeinRunner(
						(int)pos.X, (int)pos.Y,
						genRand.NextFloat(1.8f, 3),
						-diff.RotatedByRandom(0.1f),
						wallThickness * genRand.NextFloat(0.8f, 1f),
						stoneID,
						1,
						wallType: stoneWallID
					);
				}
				if (!WorldGen.remixWorldGen) {
					int lowestScore = int.MaxValue;
					Vector2 surfaceConnection = genRand.Next(cells.Where(v => v.Y < j).Select(
							v => {
								Vector2 v0 = v;
								while (Framing.GetTileSafely(v0.ToPoint()).WallType == stoneWallID) {
									v0.Y--;
								}
								int tileCount = 0;
								Tile currentTile = Framing.GetTileSafely(v0.ToPoint());
								while (currentTile.HasTile || currentTile.WallType != WallID.None) {
									v0.Y--;
									tileCount++;
									currentTile = Framing.GetTileSafely(v0.ToPoint());
								}
								if (lowestScore > tileCount) lowestScore = tileCount;
								return new Tuple<Vector2, int>(v, tileCount);
							}
						).Where(v => v.Item2 < lowestScore + 25).OrderBy(v => v.Item2).Take(3).Select(v => v.Item1).ToArray()
					);
					// make sure the surface cell has 
					{
						List<Vector2> validOthers = cells.Where(v0 => {
							if (v0 == surfaceConnection || connections.Contains((surfaceConnection, v0))) return false;
							Vector2 potentialDiff = v0 - surfaceConnection;
							float potentialLength = potentialDiff.Length();
							potentialDiff /= potentialLength;
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
						if (validOthers.Count > 0) {
							Vector2 targetCell = genRand.Next(validOthers);
							Vector2 diff = targetCell - surfaceConnection;
							float length = diff.Length();
							diff /= length;
							Vector2 pos = surfaceConnection;
							Tile currentTile = Framing.GetTileSafely(pos.ToPoint());
							while (!currentTile.HasTile) {
								pos += diff;
								currentTile = Framing.GetTileSafely(pos.ToPoint());
							}
							int wallThickness = 0;
							while (currentTile.HasTile) {
								pos += diff;
								currentTile = Framing.GetTileSafely(pos.ToPoint());
								wallThickness++;
							}
							GenRunners.WalledVeinRunner(
								(int)pos.X, (int)pos.Y,
								genRand.NextFloat(1.8f, 3),
								-diff.RotatedByRandom(0.1f),
								wallThickness * genRand.NextFloat(0.8f, 1f),
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
					GenRunners.OpeningRunner(
						(int)surfaceConnection.X, (int)surfaceConnection.Y,
						genRand.NextFloat(4, 6),
						genRand.NextFloat(0.95f, 1.2f),
						-Vector2.UnitY.RotatedByRandom(0.15f),
						75,
						validTiles
					);
				}
				if (GenVars.structures is not null) GenVars.structures.AddProtectedStructure(new Rectangle(minGenX, minGenY, maxGenX - minGenX, minGenX - minGenY), 6);
			}
			public static void SmallCave(float i, float j, float sizeMult = 1f, Vector2 stretch = default) {
				ushort stoneID = (ushort)ModContent.TileType<Sulphur_Stone>();
				ushort mossID = (ushort)ModContent.TileType<Peat_Moss_Tile>();
				ushort stoneWallID = (ushort)ModContent.WallType<Sulphur_Stone_Wall>();
				float stretchScale = stretch.Length();
				Vector2 stretchNorm = stretch / stretchScale;
				float totalSize = 20 * sizeMult * (stretchScale + 1);
				Tile tile;
				for (int x = (int)Math.Floor(i - totalSize); x < (int)Math.Ceiling(i + totalSize); x++) {
					for (int y = (int)Math.Ceiling(j - totalSize); y < (int)Math.Floor(j + totalSize); y++) {
						tile = Framing.GetTileSafely(x, y);
						if (tile.HasTile && !WorldGen.CanKillTile(x, y)) continue;
						Vector2 diff = new(y - j, x - i);
						float distSQ = diff.LengthSquared() * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1);
						float dist = (float)Math.Sqrt(distSQ);
						dist *= Math.Abs(Vector2.Dot(diff.SafeNormalize(default), stretchNorm) * stretchScale) + 1;
						if (dist > 20 * sizeMult) {
							continue;
						}
						if (tile.WallType != stoneWallID) {
							tile.ResetToType(stoneID);
						}
						tile.WallType = stoneWallID;
						if (dist < 20 * sizeMult - 10f) {
							tile.HasTile = false;
						} else if (dist < 20 * sizeMult - 9f) {
							tile.ResetToType(genRand.NextBool(5) ? TileID.Mud : mossID);
						} else if (dist < 20 * sizeMult - 7f) {
							tile.ResetToType(genRand.NextBool(5) ? mossID : TileID.Mud);
						}
						tile.LiquidType = LiquidID.Water;
						tile.LiquidAmount = 255;
						if (x > maxGenX) {
							maxGenX = x;
						}
						if (x < minGenX) {
							minGenX = x;
						}
						if (y > maxGenY) {
							maxGenY = y;
						}
						if (y < minGenY) {
							minGenY = y;
						}
					}
				}
			}
			public static Point BrineStart_Old(int i, int j, float sizeMult = 1f) {
				ushort stoneID = (ushort)ModContent.TileType<Sulphur_Stone>();
				ushort stoneWallID = WallID.BlueDungeonSlab;//(ushort)ModContent.WallType<Riven_Flesh_Wall>();
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
				Vector2 topLeft = new Vector2(i2, (float)GenVars.worldSurfaceHigh);
				Vector2 topRight = new Vector2(i2, (float)GenVars.worldSurfaceHigh);
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
	}
}
