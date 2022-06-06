using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Tiles.Defiled;
using Origins.Tiles.Dusk;
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
    public partial class OriginWorld : ModSystem {
        internal static List<(Point, int)> HellSpikes = new List<(Point, int)>() { };
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
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
            ConvertableFieldInfo<int> _JungleX = new ConvertableFieldInfo<int>(typeof(WorldGen).GetField("JungleX", BindingFlags.NonPublic | BindingFlags.Static));
            tasks.Insert(0, new PassLegacy("setting worldEvil type", (GenerationProgress progress, GameConfiguration _) =>{worldEvil|=crimson?evil_crimson:evil_corruption;}));
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
                    int JungleX = (int)_JungleX;
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
                    BrinePool.Gen.BrineStart(X, Y);
                    //}
                }));
            }
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
            Stack<Point> DefiledHearts = new Stack<Point>() { };
            if(genIndex != -1&&(WorldGen.genRand.Next(0, 2)+OriginConfig.Instance.worldTypeSkew)>0) {
                bool dungeonLeft = dungeonX < Main.maxTilesX / 2;
                int i2;
                FieldInfo grassSpread = typeof(WorldGen).GetField("grassSpread", BindingFlags.NonPublic|BindingFlags.Static);
                ushort grassType = TileID.Grass;
                ushort plantType = TileID.Plants;
                ushort stoneType = TileID.Stone;
                ushort[] stoneWallTypes = new ushort[] {WallID.Stone};
                ushort[] hardenedSandWallTypes = new ushort[] {WallID.HardenedSand};
                ushort[] sandstoneWallTypes = new ushort[] {WallID.Sandstone};
                ushort sandType = TileID.Sand;
                ushort sandstoneType = TileID.Sandstone;
                ushort hardenedSandType = TileID.HardenedSand;
                ushort iceType = TileID.IceBlock;
                List<(Point, int)> EvilSpikes = new List<(Point, int)>() { };
                tasks[genIndex] = new PassLegacy("Corruption", (GenerationProgress progress, GameConfiguration _) => {
                    int JungleX = (int)_JungleX;
                    worldEvil = crimson ? evil_riven : evil_wastelands;
                    if(crimson) {
                        getEvilTileConversionTypes(evil_riven, out stoneType, out grassType, out plantType, out sandType, out sandstoneType, out hardenedSandType, out iceType);
                        getEvilWallConversionTypes(evil_riven, out stoneWallTypes, out hardenedSandWallTypes, out sandstoneWallTypes);
                        progress.Message = Lang.gen[72].Value+"n't";
                        for(int startX = 0; startX < Main.maxTilesX * 0.00045; startX++) {
                            float attemptProgress = (float)(startX / (Main.maxTilesX * 0.00045));
                            progress.Set(attemptProgress);
                            bool validSpot = false;
                            int startPos = 0;
                            int minX = 0;
                            int maxX = 0;
                            while(!validSpot) {
                                int avoidJungleTries = 0;
                                validSpot = true;
                                int halfX = Main.maxTilesX / 2;
                                int minWidth = 200;
                                startPos = (!dungeonLeft ? genRand.Next(320, Main.maxTilesX - 600) : genRand.Next(600, Main.maxTilesX - 320));
                                minX = startPos - genRand.Next(200) - 100;
                                maxX = startPos + genRand.Next(200) + 100;
                                if(minX < 285) {
                                    minX = 285;
                                }
                                if(maxX > Main.maxTilesX - 285) {
                                    maxX = Main.maxTilesX - 285;
                                }
                                if(dungeonLeft && minX < 400) {
                                    minX = 400;
                                } else if(!dungeonLeft && minX > Main.maxTilesX - 400) {
                                    minX = Main.maxTilesX - 400;
                                }
                                if(startPos > halfX - minWidth && startPos < halfX + minWidth) {
                                    validSpot = false;
                                }
                                if(minX > halfX - minWidth && minX < halfX + minWidth) {
                                    validSpot = false;
                                }
                                if(maxX > halfX - minWidth && maxX < halfX + minWidth) {
                                    validSpot = false;
                                }
                                if(startPos > UndergroundDesertLocation.X && startPos < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
                                    validSpot = false;
                                }
                                if(minX > UndergroundDesertLocation.X && minX < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
                                    validSpot = false;
                                }
                                if(maxX > UndergroundDesertLocation.X && maxX < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
                                    validSpot = false;
                                }
                                for(int dungeonCheckX = minX; dungeonCheckX < maxX; dungeonCheckX++) {
                                    for(int dungeonCheckY = 0; dungeonCheckY < (int)Main.worldSurface; dungeonCheckY += 5) {
                                        if(Main.tile[dungeonCheckX, dungeonCheckY].HasTile && Main.tileDungeon[Main.tile[dungeonCheckX, dungeonCheckY].TileType]) {
                                            validSpot = false;
                                            break;
                                        }
                                        if(!validSpot) {
                                            break;
                                        }
                                    }
                                }
                                if(avoidJungleTries < 200 && JungleX > minX && JungleX < maxX) {
                                    avoidJungleTries++;
                                    validSpot = false;
                                }
                            }

                            bool gr = TileID.Sets.CanBeClearedDuringGeneration[TileID.Granite];
                            bool mb = TileID.Sets.CanBeClearedDuringGeneration[TileID.Marble];
                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Granite] = true;
                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Marble] = true;

                            RivenHive.Gen.StartHive(startPos, (int)worldSurfaceLow - 10);

                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Granite] = gr;
                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Marble] = mb;

                            //CrimStart(startPos, (int)worldSurfaceLow - 10);
                            for(int jungleCheckX = minX; jungleCheckX < maxX; jungleCheckX++) {
                                for(int jungleCheckY = (int)worldSurfaceLow; jungleCheckY < Main.worldSurface - 1.0; jungleCheckY++) {
                                    if(Main.tile[jungleCheckX, jungleCheckY].HasTile) {
                                        int maxDepth = jungleCheckY + genRand.Next(10, 14);
                                        for(int depth = jungleCheckY; depth < maxDepth; depth++) {
                                            if((Main.tile[jungleCheckX, depth].TileType == TileID.Mud || Main.tile[jungleCheckX, depth].TileType == TileID.JungleGrass) && jungleCheckX >= minX + genRand.Next(5) && jungleCheckX < maxX - genRand.Next(5)) {
                                                Main.tile[jungleCheckX, depth].TileType = TileID.Dirt;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            double lowSurface = Main.worldSurface + 40.0;
                            for(int currentX = minX; currentX < maxX; currentX++) {
                                lowSurface += genRand.Next(-2, 3);
                                if(lowSurface < Main.worldSurface + 30.0) {
                                    lowSurface = Main.worldSurface + 30.0;
                                }
                                if(lowSurface > Main.worldSurface + 50.0) {
                                    lowSurface = Main.worldSurface + 50.0;
                                }
                                i2 = currentX;
                                bool planted = false;
                                for(int convertY = (int)worldSurfaceLow; convertY < lowSurface; convertY++) {
                                    if(Main.tile[i2, convertY].HasTile) {
                                        if(Main.tile[i2, convertY].TileType == TileID.Sand && i2 >= minX + genRand.Next(5) && i2 <= maxX - genRand.Next(5)) {
                                            Main.tile[i2, convertY].TileType = sandType;
                                        }
                                        if(Main.tile[i2, convertY].TileType == 0 && convertY < Main.worldSurface - 1.0 && !planted) {
                                            grassSpread.SetValue(null, 0);
                                            SpreadGrass(i2, convertY, grassType, grassType, repeat: true, 0);
                                        }
                                        planted = true;
                                        switch(Main.tile[i2, convertY].WallType) {
                                            case WallID.HardenedSand:
                                            Main.tile[i2, convertY].WallType = genRand.Next(hardenedSandWallTypes);
                                            break;
                                            case WallID.Sandstone:
                                            Main.tile[i2, convertY].WallType = genRand.Next(sandstoneWallTypes);
                                            break;
                                        }
                                        switch (Main.tile[i2, convertY].TileType) {
                                            case TileID.Stone:
                                            if (i2 >= minX + genRand.Next(5) && i2 <= maxX - genRand.Next(5)) {
                                                Main.tile[i2, convertY].TileType = stoneType;
                                            }
                                            break;
                                            case TileID.Grass:
                                            Main.tile[i2, convertY].TileType = grassType;
                                            break;
                                            case TileID.IceBlock:
                                            Main.tile[i2, convertY].TileType = iceType;
                                            break;
                                            case TileID.HardenedSand:
                                            Main.tile[i2, convertY].TileType = hardenedSandType;
                                            break;
                                            case TileID.Sandstone:
                                            Main.tile[i2, convertY].TileType = sandstoneType;
                                            break;
                                        }
                                    }
                                }
                            }
                            int num503 = genRand.Next(10, 15);
                            for(int num504 = 0; num504 < num503; num504++) {
                                int num505 = 0;
                                bool flag40 = false;
                                int num506 = 0;
                                while(!flag40) {
                                    num505++;
                                    int num507 = genRand.Next(minX - num506, maxX + num506);
                                    int num508 = genRand.Next((int)(Main.worldSurface - num506 / 2), (int)(Main.worldSurface + 100.0 + num506));
                                    if(num505 > 100) {
                                        num506++;
                                        num505 = 0;
                                    }
                                    if(!Main.tile[num507, num508].HasTile) {
                                        for(; !Main.tile[num507, num508].HasTile; num508++) {
                                        }
                                        num508--;
                                    } else {
                                        while(Main.tile[num507, num508].HasTile && num508 > Main.worldSurface) {
                                            num508--;
                                        }
                                    }
                                    if(num506 > 10 || (Main.tile[num507, num508 + 1].HasTile && Main.tile[num507, num508 + 1].TileType == 203)) {
                                        Place3x2(num507, num508, TileID.DemonAltar, 0);
                                        if(Main.tile[num507, num508].TileType == 26) {
                                            flag40 = true;
                                        }
                                    }
                                    if(num506 > 100) {
                                        flag40 = true;
                                    }
                                }
                            }
                        }
                    } else {
                        defiledResurgenceTiles = new List<(int, int)> { };
                        defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
                        getEvilTileConversionTypes(evil_wastelands, out stoneType, out grassType, out plantType, out sandType, out sandstoneType, out hardenedSandType, out iceType);
                        getEvilWallConversionTypes(evil_wastelands, out stoneWallTypes, out hardenedSandWallTypes, out sandstoneWallTypes);
                        progress.Message = "Corruptionn't";
                        for(int genCount = 0; genCount < Main.maxTilesX * 0.00045; genCount++) {
                            float value15 = (float)(genCount / (Main.maxTilesX * 0.00045));
                            progress.Set(value15);
                            bool foundPosition = false;
                            int centerX = 0;
                            int genLeft = 0;
                            int genRight = 0;
                            while(!foundPosition) {
                                int jungleCheckCount = 0;
                                foundPosition = true;
                                int worldCenter = Main.maxTilesX / 2;
                                centerX = genRand.Next(320, Main.maxTilesX - 320);
                                genLeft = centerX - genRand.Next(200) - 100;
                                genRight = centerX + genRand.Next(200) + 100;
                                if(genLeft < 285) {
                                    genLeft = 285;
                                }
                                if(genRight > Main.maxTilesX - 285) {
                                    genRight = Main.maxTilesX - 285;
                                }
                                if(centerX > worldCenter - 200 && centerX < worldCenter + 200) {
                                    foundPosition = false;
                                }
                                if(genLeft > worldCenter - 200 && genLeft < worldCenter + 200) {
                                    foundPosition = false;
                                }
                                if(genRight > worldCenter - 200 && genRight < worldCenter + 200) {
                                    foundPosition = false;
                                }
                                if(centerX > UndergroundDesertLocation.X && centerX < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
                                    foundPosition = false;
                                }
                                if(genLeft > UndergroundDesertLocation.X && genLeft < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
                                    foundPosition = false;
                                }
                                if(genRight > UndergroundDesertLocation.X && genRight < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
                                    foundPosition = false;
                                }
                                for(int checkX = genLeft; checkX < genRight; checkX++) {
                                    for(int checkY = 0; checkY < (int)Main.worldSurface; checkY += 5) {
                                        if(Main.tile[checkX, checkY].HasTile && Main.tileDungeon[Main.tile[checkX, checkY].TileType]) {
                                            foundPosition = false;
                                            break;
                                        }
                                        if(!foundPosition) {
                                            break;
                                        }
                                    }
                                }
                                if(jungleCheckCount < 200 && JungleX > genLeft && JungleX < genRight) {
                                    jungleCheckCount++;
                                    foundPosition = false;
                                }
                            }
                            int num518 = 0;
                            for(int num519 = genLeft; num519 < genRight; num519++) {
                                if(num518 > 0) {
                                    num518--;
                                }
                                /*if (num519 == num510 || num518 == 0) {
						            for (int num520 = (int)worldSurfaceLow; (double)num520 < Main.worldSurface - 1.0; num520++) {
							            if (Main.tile[num519, num520].active() || Main.tile[num519, num520].wall > 0) {
								            if (num519 == num510) {
									            num518 = 20;
									            ChasmRunner(num519, num520, genRand.Next(150) + 150, makeOrb: true);
								            }
								            else if (genRand.Next(35) == 0 && num518 == 0) {
									            num518 = 30;
									            ChasmRunner(num519, num520, genRand.Next(50) + 50, makeOrb: true);
								            }
								            break;
							            }
						            }
					            }*/
                                for(int num521 = (int)worldSurfaceLow; num521 < Main.worldSurface - 1.0; num521++) {
                                    if(Main.tile[num519, num521].HasTile) {
                                        int num522 = num521 + genRand.Next(10, 14);
                                        for(int num523 = num521; num523 < num522; num523++) {
                                            if((Main.tile[num519, num523].TileType == 59 || Main.tile[num519, num523].TileType == 60) && num519 >= genLeft + genRand.Next(5) && num519 < genRight - genRand.Next(5)) {
                                                Main.tile[num519, num523].TileType = 0;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            double num524 = Main.worldSurface + 40.0;
                            for(int num525 = genLeft; num525 < genRight; num525++) {
                                num524 += genRand.Next(-2, 3);
                                if(num524 < Main.worldSurface + 30.0) {
                                    num524 = Main.worldSurface + 30.0;
                                }
                                if(num524 > Main.worldSurface + 50.0) {
                                    num524 = Main.worldSurface + 50.0;
                                }
                                i2 = num525;
                                bool flag42 = false;
                                Tile tile;
                                for(int num526 = (int)worldSurfaceLow; num526 < num524; num526++) {
                                    tile = Main.tile[i2, num526];
                                    if(tile.HasTile) {
                                        if(i2 >= genLeft + genRand.Next(5) && i2 <= genRight - genRand.Next(5)) {
                                            ConvertTile(ref tile.TileType, evil_wastelands);
                                        }
                                        if(tile.TileType == 0 && num526 < Main.worldSurface - 1.0 && !flag42) {
                                            grassSpread.SetValue(null, 0);
                                            SpreadGrass(i2, num526, TileID.Dirt, grassType, repeat: true);
                                        }
                                        flag42 = true;
                                        if(tile.WallType == WallID.Stone) {
                                            tile.WallType = genRand.Next(stoneWallTypes);
                                        } else if(tile.WallType == WallID.HardenedSand) {
                                            tile.WallType = genRand.Next(hardenedSandWallTypes);
                                        } else if(tile.WallType == WallID.Sandstone) {
                                            tile.WallType = genRand.Next(sandstoneWallTypes);
                                        }
                                        if(tile.TileType == TileID.Plants) {
                                            tile.TileType = plantType;
                                        }
                                    }
                                }
                            }
                            for(int num527 = genLeft; num527 < genRight; num527++) {
                                for(int num528 = 0; num528 < Main.maxTilesY - 50; num528++) {
                                    if(Main.tile[num527, num528].HasTile && Main.tile[num527, num528].TileType == 31) {
                                        int num529 = num527 - 13;
                                        int num530 = num527 + 13;
                                        int num531 = num528 - 13;
                                        int num532 = num528 + 13;
                                        for(int num533 = num529; num533 < num530; num533++) {
                                            if(num533 > 10 && num533 < Main.maxTilesX - 10) {
                                                for(int num534 = num531; num534 < num532; num534++) {
                                                    if(Math.Abs(num533 - num527) + Math.Abs(num534 - num528) < 9 + genRand.Next(11) && !genRand.NextBool(3)&& Main.tile[num533, num534].TileType != 31) {
                                                        Main.tile[num533, num534].HasTile = true;
                                                        Main.tile[num533, num534].TileType = 25;
                                                        if(Math.Abs(num533 - num527) <= 1 && Math.Abs(num534 - num528) <= 1) {
                                                            Main.tile[num533, num534].HasTile = false;
                                                        }
                                                    }
                                                    if(Main.tile[num533, num534].TileType != 31 && Math.Abs(num533 - num527) <= 2 + genRand.Next(3) && Math.Abs(num534 - num528) <= 2 + genRand.Next(3)) {
                                                        Main.tile[num533, num534].HasTile = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            int startY;
                            for (startY = (int)WorldGen.worldSurfaceLow; !Main.tile[centerX, startY].HasTile; startY++);
                            Point start = new Point(centerX, startY + genRand.Next(105, 150));//range of depths

                            bool gr = TileID.Sets.CanBeClearedDuringGeneration[TileID.Granite];
                            bool mb = TileID.Sets.CanBeClearedDuringGeneration[TileID.Marble];
                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Granite] = true;
                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Marble] = true;

                            DefiledWastelands.Gen.StartDefiled(start.X, start.Y);
                            DefiledHearts.Push(start);

                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Granite] = gr;
                            TileID.Sets.CanBeClearedDuringGeneration[TileID.Marble] = mb;
                        }
                    }
                });
                
                genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Altars"));
                tasks.Insert(genIndex+1, new PassLegacy("Alternate World Evil Altars", (GenerationProgress progress, GameConfiguration _) => {
                    ushort oreType = crimson ? TileID.Crimtane : TileID.Demonite;
                    ushort altOreType = (ushort)(crimson ? TileType<Infested_Ore>() : TileType<Defiled_Ore>());
                    ushort altarType = (ushort)(crimson ? TileType<Riven_Altar>() : TileType<Defiled_Altar>());
                    for(int y = 0; y < Main.maxTilesY; y++) {
                        for(int x = 0; x < Main.maxTilesX; x++) {
                            if(Main.tile[x, y].TileType==oreType)
                                Main.tile[x, y].TileType = altOreType;
                            else if(Main.tile[x, y].TileType==TileID.DemonAltar) {
                                Main.tile[x, y].TileFrameX%=54;
                                Main.tile[x, y].TileType = altarType;
                            }
                        }
                    }
                }));
                genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Weeds"));
                tasks.Insert(genIndex+1, new PassLegacy("Evil Weeds and Sand", (GenerationProgress progress, GameConfiguration _) => {
                    int tilesSinceSpike = 0;
                    for(int i = 0; i < Main.maxTilesX; i++) {
                        for(int j = 1; j < Main.maxTilesY; j++) {
                            if(Main.tile[i, j].TileType == grassType && Main.tile[i, j].HasTile && !(Main.tile[i, j].IsHalfBlock||Main.tile[i, j].Slope!=SlopeID.None)) {
                                if(!Main.tile[i, j - 1].HasTile) {
                                    PlaceTile(i, j - 1, plantType, mute: true);
                                }
                                if(worldEvil == evil_wastelands && Main.tile[i, j].HasTile && Main.tileSolid[Main.tile[i, j].TileType]) {
                                    if(genRand.Next(0, 10+EvilSpikes.Count)<=tilesSinceSpike/5) {
                                        EvilSpikes.Add((new Point(i, j), genRand.Next(9,18)+tilesSinceSpike/5));
                                        tilesSinceSpike = -15;
                                    } else {
                                        tilesSinceSpike++;
                                    }
                                }
                            } else {
                                if(Main.tile[i, j].HasTile && (!SolidTile(i, j + 1) || !SolidTile(i, j + 2))) {
                                    if(Main.tile[i, j].TileType==sandType)
                                        Main.tile[i, j].TileType = hardenedSandType;
                                }
                            }
                        }
                    }
                    if(EvilSpikes.Count>0) {
                        Mod.Logger.Info($"Adding {EvilSpikes.Count} Evil Spikes");
                    }
                    crimson = true;
                }));
                genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
                tasks.Insert(genIndex+1, new PassLegacy("Evil Biome Cleanup and Features", (GenerationProgress progress, GameConfiguration _) => {
                    while(EvilSpikes.Count>0) {
                        (Point pos, int size) i = EvilSpikes[0];
                        Point p = i.pos;
                        EvilSpikes.RemoveAt(0);
                        Vector2 vel = -GetTileDirs(p.X,p.Y).TakeAverage();
                        if(vel.Length()==0f) {
                            vel = genRand.NextVector2Circular(0.5f,0.5f);
                        } else {
                            vel = vel.RotatedByRandom(0.75f, genRand);
                        }
                        //TestRunners.SpikeRunner(p.X, p.Y, duskStoneID, vel, i.Item2, randomtwist: true);
                        double size = i.size*0.25;
                        if(genRand.NextBool(5)) {
                            size+=6;
                            Vector2 tempPos = new Vector2(p.X, p.Y);
                            while (Main.tile[(int)tempPos.X, (int)tempPos.Y].HasTile && Main.tileSolid[Main.tile[(int)tempPos.X, (int)tempPos.Y].TileType]) {
                                tempPos += vel;
                            }
                            tempPos -= vel * 3;
                            p = tempPos.ToPoint();
                            //p = new Point(p.X+(int)(vel.X*18),p.Y+(int)(vel.Y*18));
                        }
                        bool twist = genRand.NextBool();
                        GenRunners.SmoothSpikeRunner(p.X, p.Y, size, stoneType, vel, decay:genRand.NextFloat(0.15f,0.35f), twist:twist?genRand.NextFloat(-0.05f,0.05f):1f, randomtwist:!twist, cutoffStrength:1.5);
                    }
                    for(int i = 0; i < Main.maxTilesX; i++) {
                        for(int j = 1; j < Main.maxTilesY; j++) {
                            if(Main.tile[i, j].TileType == grassType && Main.tile[i, j].HasTile) {
                                if(Main.tile[i-1, j].TileType == TileID.Grass) {
                                    Main.tile[i-1, j].TileType = grassType;
                                    if(Main.tile[i-1, j-1].TileType == TileID.Plants)
                                        Main.tile[i-1, j-1].TileType = plantType;
                                }
                                if(Main.tile[i+1, j].TileType == TileID.Grass) {
                                    Main.tile[i+1, j].TileType = grassType;
                                    if(Main.tile[i+1, j-1].TileType == TileID.Plants)
                                        Main.tile[i+1, j-1].TileType = plantType;
                                }
                                if(Main.tile[i, j-1].TileType == TileID.Grass) {
                                    Main.tile[i, j-1].TileType = grassType;
                                    if(Main.tile[i, j-2].TileType == TileID.Plants)
                                        Main.tile[i, j-2].TileType = plantType;
                                }
                                if(Main.tile[i, j+1].TileType == TileID.Grass) {
                                    Main.tile[i, j+1].TileType = grassType;
                                }
                            }
                        }
                    }
                    crimson = (worldEvil&evil_crimson)!=0;
                    Point heart;
                    while(DefiledHearts.Count>0) {
                        heart = DefiledHearts.Pop();
				        DefiledWastelands.Gen.DefiledRibs(heart.X + genRand.NextFloat(-0.5f, 0.5f), heart.Y + genRand.NextFloat(-0.5f, 0.5f));
                        for (int i = heart.X - 1; i < heart.X + 3; i++) {
                            for (int j = heart.Y - 2; j < heart.Y + 2; j++) {
                                Main.tile[i, j].HasTile = false;
                            }
                        }
                        TileObject.CanPlace(heart.X, heart.Y, (ushort)TileType<Defiled_Heart>(), 0, 1, out var data);
                        TileObject.Place(data);
                        Defiled_Hearts.Add(heart);
                    }
                }));
            }
        }
        public static void GERunnerHook(On.Terraria.WorldGen.orig_GERunner orig, int i, int j, float speedX = 0f, float speedY = 0f, bool good = true) {
            byte worldEvil = GetInstance<OriginWorld>().worldEvil;
            if(!good&&(worldEvil&4)!=0) {
                ERunner(i, j, worldEvil, speedX, speedY);
                return;
            }
            orig(i, j, speedX, speedY, good);
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