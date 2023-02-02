using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
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
			public static Point BrineStart(int i, int j, float sizeMult = 1f) {
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
