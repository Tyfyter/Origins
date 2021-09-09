using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Tyfyter.Utils;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
    public static class RivenHive {
        public const int NeededTiles = 200;
        public const int ShaderTileCount = 25;
        public static class SpawnRates {
            public const float Fighter = 1;
            public const float Mummy = 1;
            public const float Jelly = 0.8f;
            public const float Tank = 0.6f;
            public const float Shark1 = 0.4f;
            public const float Crawler = 0.8f;
        }
        public static class Gen {
            static int lesionCount = 0;
            public static void StartHive(int i, int j) {
                const float strength = 3;
                const float wallThickness = 0.8f;
				OriginWorld.getEvilTileConversionTypes(OriginWorld.evil_riven, out ushort fleshID, out _, out _, out _, out _, out _, out _);
                lesionCount = 0;
                int j2 = j;
				if (j2 > Main.worldSurface) {
					j2 = (int)Main.worldSurface;
				}
				for (; !SolidTile(i, j2); j2++) {
				}
				int num = j2;
				Vector2 position = new Vector2(i, j2);
                for (int x = i - 30; x < i + 30; x++) {
                    for (int y = j2 - 25; y < j2 + 15; y++) {
                        float diff = (((y - j2) * (y - j2)*1.5f) + (x - i) * (x - i));
                        if (diff > 800) {
                            continue;
                        }
                        Main.tile[x, y].ResetToType(fleshID);
                    }
                }
                Vector2 vector = new Vector2(0, -1).RotatedByRandom(1.6f);
                int distance = 0;
                while (Main.tile[(int)position.X, (int)position.Y].active() && Main.tileSolid[Main.tile[(int)position.X, (int)position.Y].type]) {
                    Main.tile[(int)position.X, (int)position.Y].ResetToType(TileID.EmeraldGemspark);
                    SquareTileFrame((int)position.X, (int)position.Y);
                    position += vector;
                    if (++distance>=160) {
                        break;
                    }
                }
                vector = -vector;
                (Vector2 position, Vector2 velocity) last = (position, vector);
                last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedByRandom(0.5f), genRand.NextFloat(distance * 0.8f, distance * 1.2f), fleshID, 0f);
                for (int index = 0; index < 5; index++) {
                    last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedByRandom(0.8f), genRand.NextFloat(distance * 0.8f, distance * 1.2f), fleshID, wallThickness);
                    GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedBy(genRand.Next(2) * 2 - 1).RotatedByRandom(1.6f), genRand.NextFloat(distance * 0.8f, distance * 1.2f), fleshID, wallThickness);
                    PolarVec2 vel = new PolarVec2(1, last.velocity.ToRotation());
                    OriginExtensions.AngularSmoothing(ref vel.R, -MathHelper.PiOver2, 0.5f);
                    last = (last.position, (Vector2)vel);
                }
            }
        }
    }
}
