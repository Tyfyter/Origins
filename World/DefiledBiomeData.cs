using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Origins.Tiles.Defiled;
using Origins.Walls;
using Terraria;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.WorldGen;
using System;

namespace Origins.World.BiomeData {
    public static class DefiledWastelands {
        public const int NeededTiles = 200;
        public const int ShaderTileCount = 75;
        public static class SpawnRates {
            public const float Cyclops = 1;
            public const float Mite = 1;
            public const float Brute = 0.6f;
            public const float Flyer = 0.6f;
            public const float Worm = 0.6f;
            public const float Hunter = 0.6f;
		}
		public static class Gen {
			static int heartCount = 0;
			public static void StartDefiled(int i, int j) {
				const float strength = 2.4f;
				const float wallThickness = 3f;
				const float distance = 32;
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				ushort stoneWallID = (ushort)ModContent.WallType<Defiled_Stone_Wall>();
				Vector2 startVec = new Vector2(i, j);
				heartCount = 0;
				DefiledCave(i, j);
				Queue<(int generation, (Vector2 position, Vector2 velocity))> veins = new Queue<(int generation, (Vector2 position, Vector2 velocity))>();
				int startCount = genRand.Next(4, 9);
				float maxSpread = 3f / startCount;
				for (int count = startCount; count>0; count--) {
					veins.Enqueue((0, (startVec, Vector2.UnitX.RotatedBy((MathHelper.TwoPi * (count / (float)startCount)) + genRand.NextFloat(-maxSpread, maxSpread)))));
				}
				(int generation, (Vector2 position, Vector2 velocity) data) current;
				(int generation, (Vector2 position, Vector2 velocity) data) next;
				while (veins.Count > 0) {
					current = veins.Dequeue();
                    switch (genRand.Next(4)) {
						case 0:
						case 1:
						next = (current.generation+1, GenRunners.WalledVeinRunner((int)current.data.position.X, (int)current.data.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), current.data.velocity.RotatedByRandom(0.1f, genRand), genRand.NextFloat(distance * 0.8f, distance * 1.2f), stoneID, wallThickness, wallType: stoneWallID));
                        if (genRand.Next(10)> next.generation) {
							veins.Enqueue(next);
                        }
						break;
						case 2:
						next = (current.generation + 2, GenRunners.WalledVeinRunner((int)current.data.position.X, (int)current.data.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), current.data.velocity.RotatedBy(-0.3f + genRand.NextFloat(-2, 2)), genRand.NextFloat(distance * 0.8f, distance * 1.2f), stoneID, wallThickness, wallType: stoneWallID));
						if (genRand.Next(10) > next.generation) {
							veins.Enqueue(next);
						}
						next = (current.generation + 2, GenRunners.WalledVeinRunner((int)current.data.position.X, (int)current.data.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), current.data.velocity.RotatedBy(0.3f + genRand.NextFloat(-2, 2)), genRand.NextFloat(distance * 0.8f, distance * 1.2f), stoneID, wallThickness, wallType: stoneWallID));
						if (genRand.Next(10) > next.generation) {
							veins.Enqueue(next);
						}
						break;
						case 3:
						next = (current.generation + 3, GenRunners.WalledVeinRunner((int)current.data.position.X, (int)current.data.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), current.data.velocity.RotatedByRandom(0.1f, genRand), genRand.NextFloat(distance * 0.8f, distance * 1.2f), stoneID, wallThickness, wallType: stoneWallID));
						if (genRand.Next(10) > next.generation) {
							veins.Enqueue(next);
						}
						DefiledCave((int)next.data.position.X, (int)next.data.position.Y, 0.33333f);
						break;
					}
                }
				DefiledRibs(i, j);
			}
			public static void DefiledCave(int i, int j, float sizeMult = 1f) {
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				ushort stoneWallID = (ushort)ModContent.WallType<Defiled_Stone_Wall>();
				for (int x = i - (int)(28 * sizeMult + 5); x < i + (int)(28 * sizeMult + 5); x++) {
					for (int y = j + (int)(28 * sizeMult + 4); y >= j - (int)(28 * sizeMult + 4); y--) {
						float diff = (float)Math.Sqrt((((y - j) * (y - j)) + (x - i) * (x - i)) * (GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1));
						if (diff > 35 * sizeMult) {
							continue;
						}
						if (Main.tile[x, y].wall != stoneWallID) {
							Main.tile[x, y].ResetToType(stoneID);
						}
						Main.tile[x, y].wall = stoneWallID;
						if (diff < 35 * sizeMult - 5) {
							Main.tile[x, y].active(false);
						}
					}
				}
			}
			public static void DefiledRibs(int i, int j, float sizeMult = 1f) {
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				for (int x = i - (int)(28 * sizeMult + 5); x < i + (int)(28 * sizeMult + 5); x++) {
					for (int y = j + (int)(28 * sizeMult + 4); y >= j - (int)(28 * sizeMult + 4); y--) {
						float diff = (float)Math.Sqrt((((y - j) * (y - j)) + (x - i) * (x - i)) * (GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1));
						if (diff > 16 * sizeMult) {
							continue;
						}
                        if (Math.Cos(diff*0.7f)<=0) {
							Main.tile[x, y].ResetToType(stoneID);
                        } else {
							Main.tile[x, y].active(false);
						}
					}
				}
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
