using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
using Origins.Walls;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Brine_Pool : ModBiome {
		public static SpawnConditionBestiaryInfoElement BestiaryBackground { get; private set; }
		public override void Load() {
			BestiaryBackground = new SpawnConditionBestiaryInfoElement("Mods.Origins.Bestiary_Biomes.Brine_Pool", 0, "Images/MapBG1");
		}
		public override void Unload() {
			BestiaryBackground = null;
		}
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneBrine = OriginSystem.brineTiles > Brine_Pool.NeededTiles;
			return originPlayer.ZoneBrine;
		}
		public const int NeededTiles = 250;
		public const int ShaderTileCount = 75;
		public static class SpawnRates {
		}
		public static class Gen {
			//start ~100 tiles below surface
			public static void BrineStart(int i, int j) {
				float angle0 = genRand.NextFloat(MathHelper.TwoPi);
				List<Vector2> cells = new();
				for (float angle1 = genRand.NextFloat(6f, 8f); angle1 > 0; angle1 -= genRand.NextFloat(0.5f, 0.7f)) {
					float totalAngle = angle0 + angle1;
					float length = genRand.NextFloat(24f, 48f);
					genCave:
					Vector2 offset = OriginExtensions.Vec2FromPolar(totalAngle, length);
					Vector2 pos = new(i + offset.X, j + offset.Y);
					const float intolerance = 16;
					if (!cells.Any((v) => (v - pos).LengthSquared() < intolerance * intolerance)) {
						SmallCave(
							pos.X, pos.Y,
							genRand.NextFloat(1.1f, 1.2f),
							OriginExtensions.Vec2FromPolar(totalAngle, MathF.Pow(genRand.NextFloat(0.25f, 1f), 1.5f))
						);
						cells.Add(pos);
					}


					if (length < 50) {
						length += 24;
						totalAngle += (genRand.NextBool() ? 1 : -1) * genRand.NextFloat(0.6f, 1.2f);
						goto genCave;
					}
				}
				SmallCave(
					i, j,
					genRand.NextFloat(1.4f, 1.6f),
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
				int cellCount = cells.Count;
				for (int i0 = genRand.Next((int)(cellCount * 0.8f), cellCount); i0 > 0; i0--) {
					//choose random direction
					//walk from center of cell in that direction until hit block
					//directional ore runner that returns ore count
					//do it again if low total ore in cell and random chance
					//remove cell from list
				}
			}
			public static void SmallCave(float i, float j, float sizeMult = 1f, Vector2 stretch = default) {
				ushort stoneID = (ushort)ModContent.TileType<Sulphur_Stone>();
				ushort mossID = (ushort)ModContent.TileType<Peat_Moss>();
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
						}else if (dist < 20 * sizeMult - 9f) {
							tile.ResetToType(genRand.NextBool(5) ? TileID.Mud : mossID);
						} else if (dist < 20 * sizeMult - 7f) {
							tile.ResetToType(genRand.NextBool(5) ? mossID : TileID.Mud);
						}
						tile.LiquidType = LiquidID.Water;
						tile.LiquidAmount = 255;
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
				Vector2 topLeft = new Vector2(i2, (float)worldSurfaceHigh);
				Vector2 topRight = new Vector2(i2, (float)worldSurfaceHigh);
				int minX = int.MaxValue;
				int maxX = int.MinValue;
				for (int y = j2 - (int)(50 * sizeMult + 8); y > worldSurfaceLow; y--) {
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
							if (y > worldSurfaceHigh || (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])) {
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
								if (y > worldSurfaceHigh) {
									Main.tile[x, y].LiquidAmount = 255;
								}
							}
							if (y < worldSurfaceHigh && Main.tile[x, y].HasTile && change) {
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
							if (y > worldSurfaceHigh) {
								Main.tile[x, y].WallType = stoneWallID;
							}
							break;
						}
						if (change) {
							changed++;
						}
					}
					if (y < worldSurfaceHigh) {
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
					for (int y = (int)(worldSurfaceHigh + 1); y >= minY; y--) {
						Main.tile[x, y].WallType = stoneWallID;
					}
				}

				return new Point(i2, j2);
			}
		}
	}
}
