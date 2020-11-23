using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using System.Linq;
using System.Reflection;
using System;
using static Terraria.WorldGen;
using static Terraria.ModLoader.ModContent;
using Origins.Tiles;

namespace Origins.World {
    public class BiomeGen : ModWorld {
        internal static List<(Point, int)> HellSpikes = new List<(Point, int)>() {};
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
            int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Larva"));
            if(genIndex != -1) {
                int duskStoneID = TileType<Dusk_Stone>();
                tasks.Insert(genIndex + 1, new PassLegacy("HELL Biome", delegate (GenerationProgress progress) {
                    progress.Message = "Generating HELL Biome";
                    //for(int i = 0; i < Main.maxTilesX / 900; i++) {       //900 is how many biomes. the bigger is the number = less biomes
                    int X = (int)(Main.maxTilesX*0.4);//WorldGen.genRand.Next(1, Main.maxTilesX - 300);
                    TestRunners.HellRunner(X, Main.maxTilesY-25, 650, WorldGen.genRand.Next(100, 200), duskStoneID, false, 0f, 0f, true, true);
                    //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh-1).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh+1).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X-1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X+1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                    //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                    mod.Logger.Info(HellSpikes.Count+" Void Spikes: "+string.Join(", ", HellSpikes));
                    for(; HellSpikes.Count>0;) {
                        (Point, int) i = HellSpikes[0];
                        Point p = i.Item1;
                        HellSpikes.RemoveAt(0);
                        Vector2 vel = new Vector2(0, (p.Y<Main.maxTilesY-150) ? 2.75f : -2.75f).RotatedByRandom(1.25f);
                        TestRunners.SpikeRunner(p.X, p.Y, duskStoneID, vel, i.Item2, randomtwist: true);
                    }
                    //Tile tile2;
                    byte dirs = 0;
                    for(int k = TestRunners.duskLeft; k < TestRunners.duskRight; k++) {
                        for(int l = TestRunners.duskBottom; l > TestRunners.duskTop; l--) {
                            //tile2 = Main.tile[k, l];
                            if(Main.tile[k, l].type == duskStoneID) {
                                dirs = 0;
                                if(Main.tile[k-1, l].active())dirs|=1;
                                if(Main.tile[k+1, l].active())dirs|=2;
                                if(Main.tile[k, l-1].active())dirs|=4;
                                if(Main.tile[k, l+1].active())dirs|=8;
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
                                    if(Main.tile[k, l].halfBrick())break;
                                    Main.tile[k, l].slope(SlopeID.BottomLeft);
                                    break;
                                    case 10:
                                    if(Main.tile[k, l].halfBrick())break;
                                    Main.tile[k, l].slope(SlopeID.BottomRight);
                                    break;
                                    default:
                                    Main.tile[k, l].slope(0);
                                    Main.tile[k, l].halfBrick(false);
                                    break;
                                }
                            }
                        }
                    }
                    //}
                }));
                tasks.Insert(genIndex + 1, new PassLegacy("FirstLake", delegate (GenerationProgress progress) {
                    mod.Logger.Info("Generating Lake");
                    progress.Message = "Generating Lake";
                    //for (int i = 0; i < Main.maxTilesX / 5000; i++) {
                    int X = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
                    int Y = WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, (int)WorldGen.worldSurfaceHigh);
                    mod.Logger.Info("LakeGen:"+X+", "+Y);
                    //WorldGen.TileRunner(X, Y, 50, WorldGen.genRand.Next(10, 50), TileID.Stone, true, 8f, 8f, true, true);
                    WorldGen.TileRunner(X, Y, 50, WorldGen.genRand.Next(10, 50), TileID.Stone, false, 8f, 8f, true, true);
                    //WorldGen.digTunnel(X, 500, 5, 5, 10, 10, true);
                    WorldGen.digTunnel(X, Y, 3, 0, 30, 6, true);
                    //WorldGen.digTunnel(X, Y, 0, 90, 25, 50, true);
                    //}
                }));
            }
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
            if(genIndex != -1) {
                bool dungeonLeft = dungeonX < Main.maxTilesX / 2;
                int JungleX = (int)typeof(WorldGen).GetField("JungleX", BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
                int i2;
                FieldInfo grassSpread = typeof(WorldGen).GetField("grassSpread", BindingFlags.NonPublic|BindingFlags.Static);
                ushort grassType = TileID.Grass;
                ushort plantType = TileID.Plants;
                ushort stoneType = TileID.Stone;
                tasks[genIndex] = new PassLegacy("Alternate World Evil", (GenerationProgress progress) => {
                    if (crimson) {
			            progress.Message = Lang.gen[72].Value+"n't";
			            for (int num487 = 0; (double)num487 < (double)Main.maxTilesX * 0.00045; num487++) {
				            float value14 = (float)((double)num487 / ((double)Main.maxTilesX * 0.00045));
				            progress.Set(value14);
				            bool flag38 = false;
				            int num488 = 0;
				            int num489 = 0;
				            int num490 = 0;
				            while (!flag38) {
					            int num491 = 0;
					            flag38 = true;
					            int num492 = Main.maxTilesX / 2;
					            int num493 = 200;
					            num488 = (!dungeonLeft ? genRand.Next(320, Main.maxTilesX - 600) : genRand.Next(600, Main.maxTilesX - 320));
					            num489 = num488 - genRand.Next(200) - 100;
					            num490 = num488 + genRand.Next(200) + 100;
					            if (num489 < 285) {
						            num489 = 285;
					            }
					            if (num490 > Main.maxTilesX - 285) {
						            num490 = Main.maxTilesX - 285;
					            }
					            if (dungeonLeft && num489 < 400) {
						            num489 = 400;
					            }
					            else if (!dungeonLeft && num489 > Main.maxTilesX - 400) {
						            num489 = Main.maxTilesX - 400;
					            }
					            if (num488 > num492 - num493 && num488 < num492 + num493) {
						            flag38 = false;
					            }
					            if (num489 > num492 - num493 && num489 < num492 + num493) {
						            flag38 = false;
					            }
					            if (num490 > num492 - num493 && num490 < num492 + num493) {
						            flag38 = false;
					            }
					            if (num488 > UndergroundDesertLocation.X && num488 < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
						            flag38 = false;
					            }
					            if (num489 > UndergroundDesertLocation.X && num489 < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
						            flag38 = false;
					            }
					            if (num490 > UndergroundDesertLocation.X && num490 < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
						            flag38 = false;
					            }
					            for (int num494 = num489; num494 < num490; num494++) {
						            for (int num495 = 0; num495 < (int)Main.worldSurface; num495 += 5) {
							            if (Main.tile[num494, num495].active() && Main.tileDungeon[Main.tile[num494, num495].type]) {
								            flag38 = false;
								            break;
							            }
							            if (!flag38) {
								            break;
							            }
						            }
					            }
					            if (num491 < 200 && JungleX > num489 && JungleX < num490) {
						            num491++;
						            flag38 = false;
					            }
				            }
				            CrimStart(num488, (int)worldSurfaceLow - 10);
				            for (int num496 = num489; num496 < num490; num496++) {
					            for (int num497 = (int)worldSurfaceLow; (double)num497 < Main.worldSurface - 1.0; num497++) {
						            if (Main.tile[num496, num497].active()) {
							            int num498 = num497 + genRand.Next(10, 14);
							            for (int num499 = num497; num499 < num498; num499++) {
								            if ((Main.tile[num496, num499].type == 59 || Main.tile[num496, num499].type == 60) && num496 >= num489 + genRand.Next(5) && num496 < num490 - genRand.Next(5)) {
									            Main.tile[num496, num499].type = 0;
								            }
							            }
							            break;
						            }
					            }
				            }
				            double num500 = Main.worldSurface + 40.0;
				            for (int num501 = num489; num501 < num490; num501++) {
					            num500 += (double)genRand.Next(-2, 3);
					            if (num500 < Main.worldSurface + 30.0) {
						            num500 = Main.worldSurface + 30.0;
					            }
					            if (num500 > Main.worldSurface + 50.0) {
						            num500 = Main.worldSurface + 50.0;
					            }
					            i2 = num501;
					            bool planted = false;
					            for (int num502 = (int)worldSurfaceLow; (double)num502 < num500; num502++) {
						            if (Main.tile[i2, num502].active()) {
							            if (Main.tile[i2, num502].type == 53 && i2 >= num489 + genRand.Next(5) && i2 <= num490 - genRand.Next(5)) {
								            Main.tile[i2, num502].type = 234;
							            }
							            if (Main.tile[i2, num502].type == 0 && (double)num502 < Main.worldSurface - 1.0 && !planted) {
								            grassSpread.SetValue(null, 0);
								            SpreadGrass(i2, num502, 0, TileID.DiamondGemsparkOff, repeat: true, 0);
							            }
							            planted = true;
							            if (Main.tile[i2, num502].wall == 216) {
								            Main.tile[i2, num502].wall = 218;
							            }
							            else if (Main.tile[i2, num502].wall == 187) {
								            Main.tile[i2, num502].wall = 221;
							            }
							            if (Main.tile[i2, num502].type == 1) {
								            if (i2 >= num489 + genRand.Next(5) && i2 <= num490 - genRand.Next(5)) {
									            Main.tile[i2, num502].type = 203;
								            }
							            }
							            else if (Main.tile[i2, num502].type == 2) {
								            Main.tile[i2, num502].type = 199;
							            }
							            else if (Main.tile[i2, num502].type == 161) {
								            Main.tile[i2, num502].type = 200;
							            }
							            else if (Main.tile[i2, num502].type == 396) {
								            Main.tile[i2, num502].type = 401;
							            }
							            else if (Main.tile[i2, num502].type == 397) {
								            Main.tile[i2, num502].type = 399;
							            }
						            }
					            }
				            }
				            int num503 = genRand.Next(10, 15);
				            for (int num504 = 0; num504 < num503; num504++) {
					            int num505 = 0;
					            bool flag40 = false;
					            int num506 = 0;
					            while (!flag40) {
						            num505++;
						            int num507 = genRand.Next(num489 - num506, num490 + num506);
						            int num508 = genRand.Next((int)(Main.worldSurface - (double)(num506 / 2)), (int)(Main.worldSurface + 100.0 + (double)num506));
						            if (num505 > 100) {
							            num506++;
							            num505 = 0;
						            }
						            if (!Main.tile[num507, num508].active()) {
							            for (; !Main.tile[num507, num508].active(); num508++) {
							            }
							            num508--;
						            }
						            else {
							            while (Main.tile[num507, num508].active() && (double)num508 > Main.worldSurface) {
								            num508--;
							            }
						            }
						            if (num506 > 10 || (Main.tile[num507, num508 + 1].active() && Main.tile[num507, num508 + 1].type == 203)) {
							            Place3x2(num507, num508, 26, 1);
							            if (Main.tile[num507, num508].type == 26) {
								            flag40 = true;
							            }
						            }
						            if (num506 > 100) {
							            flag40 = true;
						            }
					            }
				            }
			            }
		            } else {
                        crimson = true;
                        stoneType = (ushort)TileType<Defiled_Stone>();
                        grassType = (ushort)TileType<Defiled_Grass>();
                        plantType = (ushort)TileType<Defiled_Foliage>();
			            progress.Message = "Corruptionn't";
			            for (int genCount = 0; (double)genCount < (double)Main.maxTilesX * 0.00045; genCount++) {
				            float value15 = (float)((double)genCount / ((double)Main.maxTilesX * 0.00045));
				            progress.Set(value15);
				            bool foundPosition = false;
				            int centerX = 0;
				            int genLeft = 0;
				            int genRight = 0;
				            while (!foundPosition) {
					            int jungleCheckCount = 0;
					            foundPosition = true;
					            int worldCenter = Main.maxTilesX / 2;
					            centerX = genRand.Next(320, Main.maxTilesX - 320);
					            genLeft = centerX - genRand.Next(200) - 100;
					            genRight = centerX + genRand.Next(200) + 100;
					            if (genLeft < 285) {
						            genLeft = 285;
					            }
					            if (genRight > Main.maxTilesX - 285) {
						            genRight = Main.maxTilesX - 285;
					            }
					            if (centerX > worldCenter - 200 && centerX < worldCenter + 200) {
						            foundPosition = false;
					            }
					            if (genLeft > worldCenter - 200 && genLeft < worldCenter + 200) {
						            foundPosition = false;
					            }
					            if (genRight > worldCenter - 200 && genRight < worldCenter + 200) {
						            foundPosition = false;
					            }
					            if (centerX > UndergroundDesertLocation.X && centerX < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
						            foundPosition = false;
					            }
					            if (genLeft > UndergroundDesertLocation.X && genLeft < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
						            foundPosition = false;
					            }
					            if (genRight > UndergroundDesertLocation.X && genRight < UndergroundDesertLocation.X + UndergroundDesertLocation.Width) {
						            foundPosition = false;
					            }
					            for (int checkX = genLeft; checkX < genRight; checkX++) {
						            for (int checkY = 0; checkY < (int)Main.worldSurface; checkY += 5) {
							            if (Main.tile[checkX, checkY].active() && Main.tileDungeon[Main.tile[checkX, checkY].type]) {
								            foundPosition = false;
								            break;
							            }
							            if (!foundPosition) {
								            break;
							            }
						            }
					            }
					            if (jungleCheckCount < 200 && JungleX > genLeft && JungleX < genRight) {
						            jungleCheckCount++;
						            foundPosition = false;
					            }
				            }
				            int num518 = 0;
				            for (int num519 = genLeft; num519 < genRight; num519++) {
					            if (num518 > 0) {
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
					            for (int num521 = (int)worldSurfaceLow; (double)num521 < Main.worldSurface - 1.0; num521++) {
						            if (Main.tile[num519, num521].active()) {
							            int num522 = num521 + genRand.Next(10, 14);
							            for (int num523 = num521; num523 < num522; num523++) {
								            if ((Main.tile[num519, num523].type == 59 || Main.tile[num519, num523].type == 60) && num519 >= genLeft + genRand.Next(5) && num519 < genRight - genRand.Next(5)) {
									            Main.tile[num519, num523].type = 0;
								            }
							            }
							            break;
						            }
					            }
				            }
				            double num524 = Main.worldSurface + 40.0;
				            for (int num525 = genLeft; num525 < genRight; num525++) {
					            num524 += (double)genRand.Next(-2, 3);
					            if (num524 < Main.worldSurface + 30.0) {
						            num524 = Main.worldSurface + 30.0;
					            }
					            if (num524 > Main.worldSurface + 50.0) {
						            num524 = Main.worldSurface + 50.0;
					            }
					            i2 = num525;
					            bool flag42 = false;
                                Tile tile;
					            for (int num526 = (int)worldSurfaceLow; (double)num526 < num524; num526++) {
                                    tile = Main.tile[i2, num526];
						            if (tile.active()) {
							            if (tile.type == TileID.Sand && i2 >= genLeft + genRand.Next(5) && i2 <= genRight - genRand.Next(5)) {
								            tile.type = TileID.Silt;

							            }
							            if (tile.type == 0 && (double)num526 < Main.worldSurface - 1.0 && !flag42) {
								            grassSpread.SetValue(null, 0);
								            SpreadGrass(i2, num526, TileID.Dirt, grassType, repeat: true);
							            }
							            flag42 = true;
							            if (tile.type == TileID.Stone && i2 >= genLeft + genRand.Next(5) && i2 <= genRight - genRand.Next(5)) {
                                            tile.type = stoneType;
							            }
							            if (tile.wall == WallID.HardenedSand) {
								            //tile.wall = WallID.HallowHardenedSand;
                                            tile.color(26);
							            }
							            else if (tile.wall == WallID.Sandstone) {
								            //tile.wall = WallID.HallowSandstone;
                                            tile.color(26);
							            }
							            if (tile.type == TileID.Plants) {
								            tile.type = grassType;
                                            //tile.color(26);
							            } else if (tile.type == TileID.Grass || tile.type == TileID.JungleGrass || tile.type == TileID.MushroomGrass) {
								            tile.type = grassType;
                                            //tile.color(26);
							            } else if (tile.type == TileID.IceBlock) {
                                            tile.color(27);
                                            //tile.type = TileID.Stone;
                                            //WorldGen.paintTile(i2, num526, 26);
							            } else if (tile.type == TileID.Sandstone) {
								            //tile.type = TileID.HallowSandstone;
                                            tile.color(26);
							            }else if (tile.type == TileID.HardenedSand) {
								            //tile.type = TileID.HallowHardenedSand;
                                            tile.color(26);
							            }
						            }
					            }
				            }
				            for (int num527 = genLeft; num527 < genRight; num527++) {
					            for (int num528 = 0; num528 < Main.maxTilesY - 50; num528++) {
						            if (Main.tile[num527, num528].active() && Main.tile[num527, num528].type == 31) {
							            int num529 = num527 - 13;
							            int num530 = num527 + 13;
							            int num531 = num528 - 13;
							            int num532 = num528 + 13;
							            for (int num533 = num529; num533 < num530; num533++) {
								            if (num533 > 10 && num533 < Main.maxTilesX - 10) {
									            for (int num534 = num531; num534 < num532; num534++) {
										            if (Math.Abs(num533 - num527) + Math.Abs(num534 - num528) < 9 + genRand.Next(11) && genRand.Next(3) != 0 && Main.tile[num533, num534].type != 31) {
											            Main.tile[num533, num534].active(active: true);
											            Main.tile[num533, num534].type = 25;
											            if (Math.Abs(num533 - num527) <= 1 && Math.Abs(num534 - num528) <= 1) {
												            Main.tile[num533, num534].active(active: false);
											            }
										            }
										            if (Main.tile[num533, num534].type != 31 && Math.Abs(num533 - num527) <= 2 + genRand.Next(3) && Math.Abs(num534 - num528) <= 2 + genRand.Next(3)) {
											            Main.tile[num533, num534].active(active: false);
										            }
									            }
								            }
							            }
						            }
					            }
				            }
			            }
		            }});
            }
        }
    }
}