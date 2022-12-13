using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Tiles.Defiled;
using Origins.Tiles.Dusk;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace Origins {
    public partial class OriginSystem : ModSystem {
        internal static List<(Point, int)> HellSpikes = new List<(Point, int)>() { };
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge = new();
            Defiled_Wastelands_Alt_Biome.defiledWastelandsEastEdge = new();

            #region _
            /*genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            if (genIndex == -1) {
                return;
            }
            tasks.Insert(genIndex + 1, new PassLegacy("TEST Biome", delegate (GenerationProgress progress) {
            progress.Message = "Generating TEST Biome";
            for(int i = 0; i < Main.maxTilesX / 900; i++) {       //900 is how many biomes. the bigger is the number = less biomes
                int X = (int)(Main.maxTilesX*0.4);//WorldGen.genRand.Next(1, Main.maxTilesX - 300);
                int Y = (int)WorldGen.worldSurfaceLow;//WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY - 200);
                int TileType = 56;     //this is the tile u want to use for the biome , if u want to use a vanilla tile then its int TileType = 56; 56 is obsidian block

                WorldGen.TileRunner(X, Y, 350, WorldGen.genRand.Next(100, 200), TileType, false, 0f, 0f, true, true);  //350 is how big is the biome     100, 200 this changes how random it looks.
                WorldGen.CloudLake(X, (int)WorldGen.worldSurfaceHigh);
                WorldGen.AddShadowOrb(X, (int) WorldGen.worldSurfaceHigh-1);
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh-1).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh+1).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X-1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X+1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
            }
            }));*/
            #endregion _
            int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Larva"));
            if(genIndex != -1) {
                int duskStoneID = TileType<Dusk_Stone>();
                tasks.Insert(genIndex + 1, new PassLegacy("Dusk Biome", delegate (GenerationProgress progress, GameConfiguration _) {
                    progress.Message = "Generating Dusk Biome";
                    //for(int i = 0; i < Main.maxTilesX / 900; i++) {       //900 is how many biomes. the bigger is the number = less biomes
                    int X = (int)(Main.maxTilesX*0.4);//WorldGen.genRand.Next(1, Main.maxTilesX - 300);
                    GenRunners.HellRunner(X, Main.maxTilesY-25, 650, WorldGen.genRand.Next(100, 200), duskStoneID, false, 0f, 0f, true, true);
                    //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh-1).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh+1).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X-1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X+1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                    Mod.Logger.Info(HellSpikes.Count+" Void Spikes: "+string.Join(", ", HellSpikes));
                    while(HellSpikes.Count>0) {
                        (Point, int) i = HellSpikes[0];
                        Point p = i.Item1;
                        HellSpikes.RemoveAt(0);
                        Vector2 vel = new Vector2(0, (p.Y<Main.maxTilesY-150) ? 2.75f : -2.75f).RotatedByRandom(1.25f, genRand);
                        //TestRunners.SpikeRunner(p.X, p.Y, duskStoneID, vel, i.Item2, randomtwist: true);
                        bool twist = genRand.NextBool();
                        GenRunners.SmoothSpikeRunner(p.X, p.Y, i.Item2*0.75, duskStoneID, vel, decay:genRand.NextFloat(0.75f,1f), twist:twist?0.3f:0, randomtwist: twist, cutoffStrength:1);
                    }
                    //Tile tile2;
                    //byte dirs = 0;
                    for(int k = GenRunners.duskLeft; k < GenRunners.duskRight; k++) {
                        for(int l = GenRunners.duskBottom; l > GenRunners.duskTop; l--) {
                            //tile2 = Main.tile[k, l];
                            if(Main.tile[k, l].TileType == duskStoneID) {
                                GenRunners.AutoSlope(k,l,true);
                                /*dirs = 0;
                                if(Main.tile[k-1, l].active())
                                    dirs|=1;
                                if(Main.tile[k+1, l].active())
                                    dirs|=2;
                                if(Main.tile[k, l-1].active())
                                    dirs|=4;
                                if(Main.tile[k, l+1].active())
                                    dirs|=8;
                                switch(dirs) {
                                    //top slopes
                                    case 6:
                                    Main.tile[k, l].slope(SlopeID.TopLeft);
                                    Main.tile[k, l].halfBrick(false);
                                    break;
                                    case 9:
                                    Main.tile[k, l].slope(SlopeID.TopRight);
                                    Main.tile[k, l].halfBrick(false);
                                    break;
                                    //bottom slopes
                                    case 5:
                                    if(Main.tile[k, l].halfBrick())
                                        break;
                                    Main.tile[k, l].slope(SlopeID.BottomLeft);
                                    break;
                                    case 10:
                                    if(Main.tile[k, l].halfBrick())
                                        break;
                                    Main.tile[k, l].slope(SlopeID.BottomRight);
                                    break;
                                    default:
                                    Main.tile[k, l].slope(0);
                                    Main.tile[k, l].halfBrick(false);
                                    break;
                                }*/
                            }
                        }
                    }
                    //}
                }));
                tasks.Insert(genIndex + 1, new PassLegacy("Brine Pool", delegate (GenerationProgress progress, GameConfiguration _) {
                    Mod.Logger.Info("Pooling Brine");
                    progress.Message = "Pooling Brine";
                    //for (int i = 0; i < Main.maxTilesX / 5000; i++) {
                    int X = WorldGen.genRand.Next(JungleX - 100, JungleX + 100);
                    int Y;
                    for (Y = (int)WorldGen.worldSurfaceLow; !Main.tile[X, Y].HasTile; Y++);
                    Y += WorldGen.genRand.Next(100, 200);
                    Mod.Logger.Info("BrineGen:" + X+", "+Y);
                    //WorldGen.TileRunner(X, Y, 50, WorldGen.genRand.Next(10, 50), TileID.Stone, true, 8f, 8f, true, true);
                    //WorldGen.TileRunner(X, Y, 50, WorldGen.genRand.Next(10, 50), TileID.Stone, false, 8f, 8f, true, true);
                    //WorldGen.digTunnel(X, 500, 5, 5, 10, 10, true);
                    //WorldGen.digTunnel(X, Y, 3, 0, 30, 6, true);
                    //WorldGen.digTunnel(X, Y, 0, 90, 25, 50, true);
                    Brine_Pool.Gen.BrineStart(X, Y);
                    //}
                }));
            }
            List<(Point, int)> EvilSpikes = new List<(Point, int)>() { };
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Weeds"));
            getEvilTileConversionTypes(evil_wastelands, out ushort defiledStoneType, out ushort defiledGrassType, out ushort defiledPlantType, out ushort defiledSandType, out ushort _, out ushort _, out ushort defilediceType);
            tasks.Insert(genIndex + 1, new PassLegacy("Finding Spots For Spikes", (GenerationProgress progress, GameConfiguration _) => {
				for (int index = 0; index < Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge.Count; index++) {
                    int minX = Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge[index];
                    int maxX = Defiled_Wastelands_Alt_Biome.defiledWastelandsEastEdge[index];
                    int tilesSinceSpike = 0;
                    for (int i = minX; i <= maxX; i++) {
                        int heightSinceSurface = 0;
                        bool canSpike = true;
                        for (int j = 1; j < Main.maxTilesY; j++) {
                            if (Main.tile[i, j].HasTile && !(Main.tile[i, j].IsHalfBlock || Main.tile[i, j].Slope != SlopeID.None)) {
                                if (Main.tile[i, j].TileType == TileID.Plants) {
                                    Main.tile[i, j].TileType = defiledPlantType;
                                }
                                if (Main.tile[i, j].TileType == TileID.Grass) {
                                    Main.tile[i, j].TileType = defiledGrassType;
								}
								if (canSpike && (Main.tile[i, j].TileType == defiledStoneType
                                    || Main.tile[i, j].TileType == defiledGrassType
                                    || Main.tile[i, j].TileType == defiledSandType
                                    || Main.tile[i, j].TileType == defilediceType
                                    || Main.tile[i, j].TileType == TileID.SnowBlock)) {
                                    if (genRand.Next(0, 10 + EvilSpikes.Count) <= tilesSinceSpike / 5) {
                                        EvilSpikes.Add((new Point(i, j), genRand.Next(9, 18) + tilesSinceSpike / 5));
                                        tilesSinceSpike = -15;
                                        canSpike = false;
                                    } else {
                                        tilesSinceSpike++;
                                    }
                                }
								if (++heightSinceSurface > 30) {
                                    break;
								}
                            }
                        }
                    }
                }
                if (EvilSpikes.Count > 0) {
                    Mod.Logger.Info($"Adding {EvilSpikes.Count} Evil Spikes");
                }
                crimson = true;
            }));
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            tasks.Insert(genIndex + 1, new PassLegacy("Placing Spikes", (GenerationProgress progress, GameConfiguration _) => {
                while (EvilSpikes.Count > 0) {
                    (Point pos, int size) i = EvilSpikes[0];
                    Point p = i.pos;
                    EvilSpikes.RemoveAt(0);
                    Vector2 vel = -GetTileDirs(p.X, p.Y).TakeAverage();
                    if (vel.Length() == 0f) {
                        vel = genRand.NextVector2Circular(0.5f, 0.5f);
                    } else {
                        vel = vel.RotatedByRandom(0.75f, genRand);
                    }
                    //TestRunners.SpikeRunner(p.X, p.Y, duskStoneID, vel, i.Item2, randomtwist: true);
                    double size = i.size * 0.25;
                    if (genRand.NextBool(5)) {
                        size += 6;
                        Vector2 tempPos = new Vector2(p.X, p.Y);
                        while (Main.tile[(int)tempPos.X, (int)tempPos.Y].HasTile && Main.tileSolid[Main.tile[(int)tempPos.X, (int)tempPos.Y].TileType]) {
                            tempPos += vel;
                        }
                        tempPos -= vel * 3;
                        p = tempPos.ToPoint();
                        //p = new Point(p.X+(int)(vel.X*18),p.Y+(int)(vel.Y*18));
                    }
                    bool twist = genRand.NextBool();
                    GenRunners.SmoothSpikeRunner(p.X, p.Y, size, defiledStoneType, vel, decay: genRand.NextFloat(0.15f, 0.35f), twist: twist ? genRand.NextFloat(-0.05f, 0.05f) : 1f, randomtwist: !twist, cutoffStrength: 1.5);
                }
            }));
            tasks.Add(new PassLegacy("Stone Mask", (GenerationProgress progress, GameConfiguration _) => {
                int i = 0;
				while (i < 100) {
                    int x = genRand.Next(oceanDistance, Main.maxTilesX - oceanDistance);
                    int y = 0;
                    for (; !Main.tile[x, y + 1].HasTile; y++);
					if (Main.tileSolid[Main.tile[x, y + 1].TileType] && Main.tileSolid[Main.tile[x + 1, y + 1].TileType]) {
						if (PlaceTile(x, y, TileType<Stone_Mask_Tile>())) {
                            break;
						}
					}
				}
            }));
        }

		public static void GERunnerHook(On.Terraria.WorldGen.orig_GERunner orig, int i, int j, float speedX = 0f, float speedY = 0f, bool good = true) {
            byte worldEvil = GetInstance<OriginSystem>().worldEvil;
            if(!good&&(worldEvil&4)!=0) {
                ERunner(i, j, worldEvil, speedX, speedY);
            }else if (good) {
                GRunner(i, j, speedX, speedY);
			} else {
                orig(i, j, speedX, speedY, good);
            }
        }
        static void ERunner(int i, int j, byte worldEvil, float speedX = 0f, float speedY = 0f) {
            int size = genRand.Next(200, 250);
            float sizeMult = Main.maxTilesX / 4200;
            size = (int)(size * sizeMult);
            Vector2 pos = default;
            pos.X = i;
            pos.Y = j;
            Vector2 velocity = default;
            velocity.X = genRand.Next(-10, 11) * 0.1f;
            velocity.Y = genRand.Next(-10, 11) * 0.1f;
            if(speedX != 0f || speedY != 0f) {
                velocity.X = speedX;
                velocity.Y = speedY;
            }
            int loopCount = 0;
            long conversionCount = 0;
            bool cont = true;
            while(cont) {
                int minX = (int)(pos.X - size * 0.5);
                int maxX = (int)(pos.X + size * 0.5);
                int minY = (int)(pos.Y - size * 0.5);
                int maxY = (int)(pos.Y + size * 0.5);
                if(minX < 0) {
                    minX = 0;
                }
                if(maxX > Main.maxTilesX) {
                    maxX = Main.maxTilesX;
                }
                if(minY < 0) {
                    minY = 0;
                }
                if(maxY > Main.maxTilesY) {
                    maxY = Main.maxTilesY;
                }
                for(int k = minX; k < maxX; k++) {
                    for(int l = minY; l < maxY; l++) {
                        if(!(Math.Abs(k - pos.X) + Math.Abs(l - pos.Y) < size * 0.5 * (1.0 + genRand.Next(-10, 11) * 0.015))) {
                            continue;
                        }
                        if(Main.tile[k, l].WallType == 63 || Main.tile[k, l].WallType == 65 || Main.tile[k, l].WallType == 66 || Main.tile[k, l].WallType == 68) {
                            //Main.tile[k, l].wall = 69;//unsafe grass wall
                        } else if(Main.tile[k, l].WallType == 216) {
                            //Main.tile[k, l].wall = 217;//hardened sand
                        } else if(Main.tile[k, l].WallType == 187) {
                            //Main.tile[k, l].wall = WallID;//sandstone
                        }
                        if(ConvertTile(ref Main.tile[k, l].TileType, worldEvil, true)) {
                            SquareTileFrame(k, l);
                            conversionCount++;
                        }
                    }
                }
                pos += velocity;
                velocity.X += genRand.Next(-10, 11) * 0.05f;
                if(velocity.X > speedX + 1f) {
                    velocity.X = speedX + 1f;
                }
                if(velocity.X < speedX - 1f) {
                    velocity.X = speedX - 1f;
                }
                if(pos.X < (-size) || pos.Y < -size || pos.X > Main.maxTilesX + size || pos.Y > Main.maxTilesX + size) {
                    cont = false;
                }
                loopCount++;
            }
            Origins.instance.Logger.Info("hardmode evil stripe generation complete in "+loopCount+" loops with "+conversionCount+" tile conversions");
        }
        static void GRunner(int i, int j, float speedX = 0f, float speedY = 0f) {
            int size = genRand.Next(200, 250);
            float sizeMult = Main.maxTilesX / 4200;
            size = (int)(size * sizeMult);
            Vector2 pos = default;
            pos.X = i;
            pos.Y = j;
            Vector2 velocity = default;
            velocity.X = genRand.Next(-10, 11) * 0.1f;
            velocity.Y = genRand.Next(-10, 11) * 0.1f;
            if (speedX != 0f || speedY != 0f) {
                velocity.X = speedX;
                velocity.Y = speedY;
            }
            int loopCount = 0;
            long conversionCount = 0;
            bool cont = true;
            while (cont) {
                int minX = (int)(pos.X - size * 0.5);
                int maxX = (int)(pos.X + size * 0.5);
                int minY = (int)(pos.Y - size * 0.5);
                int maxY = (int)(pos.Y + size * 0.5);
                if (minX < 0) {
                    minX = 0;
                }
                if (maxX > Main.maxTilesX) {
                    maxX = Main.maxTilesX;
                }
                if (minY < 0) {
                    minY = 0;
                }
                if (maxY > Main.maxTilesY) {
                    maxY = Main.maxTilesY;
                }
                for (int k = minX; k < maxX; k++) {
                    for (int l = minY; l < maxY; l++) {
                        if (!(Math.Abs(k - pos.X) + Math.Abs(l - pos.Y) < size * 0.5 * (1.0 + genRand.Next(-10, 11) * 0.015))) {
                            continue;
                        }
                        if (Main.tile[k, l].WallType == 63 || Main.tile[k, l].WallType == 65 || Main.tile[k, l].WallType == 66 || Main.tile[k, l].WallType == 68) {
                            //Main.tile[k, l].wall = 69;//unsafe grass wall
                        } else if (Main.tile[k, l].WallType == 216) {
                            //Main.tile[k, l].wall = 217;//hardened sand
                        } else if (Main.tile[k, l].WallType == 187) {
                            //Main.tile[k, l].wall = WallID;//sandstone
                        }
                        if (HallowConvertTile(ref Main.tile[k, l].TileType, true)) {
                            SquareTileFrame(k, l);
                            conversionCount++;
                        }
                    }
                }
                pos += velocity;
                velocity.X += genRand.Next(-10, 11) * 0.05f;
                if (velocity.X > speedX + 1f) {
                    velocity.X = speedX + 1f;
                }
                if (velocity.X < speedX - 1f) {
                    velocity.X = speedX - 1f;
                }
                if (pos.X < (-size) || pos.Y < -size || pos.X > Main.maxTilesX + size || pos.Y > Main.maxTilesX + size) {
                    cont = false;
                }
                loopCount++;
            }
            Origins.instance.Logger.Info("hardmode hallow stripe generation complete in " + loopCount + " loops with " + conversionCount + " tile conversions");
        }
        public static void RemoveTree(int i, int j) {
            Tile tile = Main.tile[i, j];

            while (tile.TileIsType(TileID.Trees)) {
				if (Main.tile[i - 1, j].TileIsType(TileID.Trees)) {
                    Main.tile[i - 1, j].ClearTile();
                }
                if (Main.tile[i + 1, j].TileIsType(TileID.Trees)) {
                    Main.tile[i + 1, j].ClearTile();
                }
                tile.HasTile = false;
                tile = Main.tile[i, --j];
            }
		}
        public static void getEvilTileConversionTypes(byte evilType, out ushort stoneType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType) {
            switch(evilType) {
                case evil_wastelands:
                stoneType = (ushort)TileType<Defiled_Stone>();
                grassType = (ushort)TileType<Defiled_Grass>();
                plantType = (ushort)TileType<Defiled_Foliage>();
                sandType = (ushort)TileType<Defiled_Sand>();
                sandstoneType = (ushort)TileType<Defiled_Sandstone>();
                hardenedSandType = (ushort)TileType<Hardened_Defiled_Sand>();
                iceType = (ushort)TileType<Defiled_Ice>();
                break;
                case evil_riven:
                stoneType = (ushort)TileType<Riven_Flesh>();
                grassType = stoneType;
                plantType = (ushort)TileType<Riven_Foliage>();
                sandType = stoneType;
                sandstoneType = stoneType;
                hardenedSandType = stoneType;
                iceType = stoneType;
                break;
                case evil_crimson:
                stoneType = TileID.Crimstone;
                grassType = TileID.CrimsonGrass;
                plantType = TileID.CrimsonPlants;
                sandType = TileID.Crimsand;
                sandstoneType = TileID.CrimsonSandstone;
                hardenedSandType = TileID.CrimsonHardenedSand;
                iceType = TileID.FleshIce;
                break;
                default:
                stoneType = TileID.Ebonstone;
                grassType = TileID.CorruptGrass;
                plantType = TileID.CorruptPlants;
                sandType = TileID.Ebonsand;
                sandstoneType = TileID.CorruptSandstone;
                hardenedSandType = TileID.CorruptHardenedSand;
                iceType = TileID.CorruptIce;
                break;
            }
        }
        public static void getEvilWallConversionTypes(byte evilType, out ushort[] stoneWallTypes, out ushort[] hardenedSandWallTypes, out ushort[] sandstoneWallTypes) {
            switch(evilType) {
                case evil_wastelands:
                stoneWallTypes = new ushort[] { (ushort)WallType<Defiled_Stone_Wall>() };
                hardenedSandWallTypes = new ushort[] { (ushort)WallType<Hardened_Defiled_Sand_Wall>() };
                sandstoneWallTypes = new ushort[] { (ushort)WallType<Defiled_Sandstone_Wall>() };
                break;
                case evil_riven:
                stoneWallTypes = new ushort[] { WallID.CrimstoneUnsafe };
                hardenedSandWallTypes = new ushort[] { WallID.CrimsonHardenedSand };
                sandstoneWallTypes = new ushort[] { WallID.CrimsonSandstone };
                break;
                case evil_crimson:
                stoneWallTypes = new ushort[] { WallID.CrimstoneUnsafe };
                hardenedSandWallTypes = new ushort[] { WallID.CrimsonHardenedSand };
                sandstoneWallTypes = new ushort[] { WallID.CrimsonSandstone };
                break;
                default:
                stoneWallTypes = new ushort[] { WallID.EbonstoneUnsafe };
                hardenedSandWallTypes = new ushort[] { WallID.CorruptHardenedSand };
                sandstoneWallTypes = new ushort[] { WallID.CorruptSandstone };
                break;
            }
        }
    }
}