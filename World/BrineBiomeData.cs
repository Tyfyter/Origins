using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
    public static class BrinePool {
        public const int NeededTiles = 250;
        public const int ShaderTileCount = 75;
        public static class SpawnRates {
        }
        public static class Gen {
			public static Point BrineStart(int i, int j, float sizeMult = 1f) {
				ushort stoneID = (ushort)ModContent.TileType<Sulphur_Stone>();
				ushort stoneWallID = WallID.BlueDungeonSlab;//(ushort)ModContent.WallType<Riven_Flesh_Wall>();
				int i2 = i + (int)(genRand.Next(-32, 32) * sizeMult);
				int j2 = j + (int)(44 * sizeMult);
				for (int x = i2 - (int)(66 * sizeMult + 10); x < i2 + (int)(66 * sizeMult + 10); x++) {
					for (int y = j2 + (int)(56 * sizeMult + 8); y >= j2 - (int)(56 * sizeMult + 8); y--) {
						float sq = Math.Max(Math.Abs(y - j2) * 1.5f, Math.Abs(x - i2));
						float pyth = (((y - j2) * (y - j2) * 1.5f) + (x - i2) * (x - i2));
						//define the distance between the point and center as a combination of Euclidian distance (dist = sqrt(xdist² + ydist²)) and Chebyshev distance (dist = max(xdist, ydist))
						float diff = (float)Math.Sqrt((sq * sq + (pyth * 3)) * 0.25f * (GetWallDistOffset(x) * 0.0316076058772687986171132238548f + 1));
						if (diff > 70 * sizeMult) {
							continue;
						}

						switch (Main.tile[x, y].type) {
							case TileID.IridescentBrick:
							case TileID.TinBrick:
							case TileID.GoldBrick:
							case TileID.Mudstone:
							if (Main.tileContainer[Main.tile[x, y - 1].type] || genRand.Next(5) > 0) {
								break;
							}
							goto default;
							case TileID.LivingMahogany:
							Main.tile[x, y].type = TileID.Ash;
							break;
							default:
                            if (Main.tileContainer[Main.tile[x, y].type]) {
								break;
                            }
							Main.tile[x, y].ResetToType(stoneID);
							if (diff < 70 * sizeMult - 10 || ((y - j) * (y - j)) + ((x - i) * (x - i) * 0.5f) < 700 * sizeMult * sizeMult) {//(x - i) * 
								if (Main.tileContainer[Main.tile[x, y - 1].type]) {
									break;
								}
								Main.tile[x, y].active(false);
								if (y > j2 - (sizeMult * 32)) {
									Main.tile[x, y].liquid = 255;
								}
							}
							break;
						}
						switch (Main.tile[x, y].wall) {
							case WallID.IridescentBrick:
							case WallID.TinBrick:
							case WallID.GoldBrick:
							case WallID.MudstoneBrick:
							if (genRand.Next(5) == 0) {
								goto default;
							}
							break;
							default:
							Main.tile[x, y].wall = stoneWallID;
							break;
						}
					}
				}
				return new Point(i2, j2);
			}
			public static float GetWallDistOffset(float value) {
				float x = value * 0.4f;
				float halfx = x * 0.5f;
				float quarx = x * 0.5f;
				float fx0 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				halfx += 0.5f;
				float fx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				float fx2 = fx0 * (float)(Math.Min(Math.Pow(quarx % 3, quarx % 5), 2) + 0.5f);
				return fx0 - fx2 + fx1;
			}
		}
    }
}
