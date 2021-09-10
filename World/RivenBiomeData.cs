using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Origins.Walls;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
                const float strength = 2.4f;
                const float wallThickness = 4f;
                ushort fleshID = (ushort)ModContent.TileType<Riven_Flesh>();
                ushort weakFleshID = (ushort)ModContent.TileType<Weak_Riven_Flesh>();
                ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
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
                        float diff = (((y - j2) * (y - j2) * 1.5f) + (x - i) * (x - i));
                        if (diff > 800) {
                            continue;
                        }
                        Main.tile[x, y].ResetToType(fleshID);
                        if (diff < 750) {
                            Main.tile[x, y].wall = fleshWallID;
                        }
                    }
                }
                Vector2 vector = new Vector2(0, -1).RotatedByRandom(1.6f);
                int distance = 0;
                while (Main.tile[(int)position.X, (int)position.Y].active() && Main.tileSolid[Main.tile[(int)position.X, (int)position.Y].type]) {
                    Main.tile[(int)position.X, (int)position.Y].ResetToType(TileID.EmeraldGemspark);
                    SquareTileFrame((int)position.X, (int)position.Y);
                    position += vector;
                    if (++distance >= 160) {
                        break;
                    }
                }
                vector = -vector;
                (Vector2 position, Vector2 velocity) last = (position, vector);
                //Tile t = Main.tile[(int)last.position.X, (int)last.position.Y];
                last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedByRandom(0.5f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness);
                //t.ResetToType(TileID.AmethystGemspark);
                Vector2 manualVel = new Vector2(last.velocity.X, 0.2f);
                //t = Main.tile[(int)last.position.X, (int)last.position.Y];
                GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(new Vector2(-manualVel.X, 0.2f)), genRand.NextFloat(distance * 0.4f, distance * 0.6f) * (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType:fleshWallID);
                last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
                GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
                last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, new Vector2(0, 1).RotatedByRandom(0.2f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
                //t.ResetToType(TileID.AmethystGemspark);
                manualVel.X = -manualVel.X;
                //t = Main.tile[(int)last.position.X, (int)last.position.Y];
                GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(new Vector2(-manualVel.X, 0.2f)), genRand.NextFloat(distance * 0.4f, distance * 0.6f) * (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
                last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
                GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
                last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, new Vector2(0, 1).RotatedByRandom(0.2f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
                //t.ResetToType(TileID.AmethystGemspark);
                for (int index = 0; index < 10; index++) {
                    //t = Main.tile[(int)last.position.X, (int)last.position.Y];
                    last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedByRandom(0.8f), genRand.NextFloat(distance * 0.2f, distance * 0.3f), weakFleshID, wallThickness, wallType: fleshWallID);
                    if (index < 8) {
                        GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedBy(genRand.Next(2) * 2 - 1).RotatedByRandom(0.8f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
                    }
                    PolarVec2 vel = new PolarVec2(1, last.velocity.ToRotation());
                    OriginExtensions.AngularSmoothing(ref vel.Theta, MathHelper.PiOver2, 0.7f);
                    //t.ResetToType(TileID.AmethystGemspark);
                    last = (last.position, (Vector2)vel);
                }
                //t = Main.tile[(int)last.position.X, (int)last.position.Y];
                //t.ResetToType(TileID.AmethystGemspark);
                HiveCave((int)last.position.X, (int)last.position.Y);
            }
            public static void HiveCave(int i, int j) {
                ushort fleshID = (ushort)ModContent.TileType<Riven_Flesh>();
                ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
                int i2 = i + genRand.Next(-24, 24);
                int j2 = j + 16;
                for (int x = i2 - 30; x < i2 + 30; x++) {
                    for (int y = j2 - 24; y < j2 + 24; y++) {
                        float sq = Math.Max(Math.Abs(y - j2) * 1.5f, Math.Abs(x - i2));
                        float diff = (float)(sq * sq + (((y - j2) * (y - j2) * 1.5f) + (x - i2) * (x - i2))) * 0.5f * genRand.NextFloat(0.9f,1.1f);
                        if (diff > 800) {
                            continue;
                        }
                        Main.tile[x, y].ResetToType(fleshID);
                        Main.tile[x, y].wall = fleshWallID;
                        if (diff < 600 || ((y - j) * (y - j)) + (x - i) * (x - i) < 25) {
                            Main.tile[x, y].active(false);
                        }
                    }
                }
            }
        }
    }
}
