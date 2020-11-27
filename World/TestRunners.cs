using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;

namespace Origins.World {
    class TestRunners{
        public static Point SpikeRunner(int i, int j, int type, Vector2 speed, int size, float twist = 0, bool randomtwist = false) {
            float x = i;
            float y = j;
            while(size>0) {
                WorldGen.TileRunner((int)x, (int)y, size, 2, type, speedX:speed.X, speedY:speed.Y, addTile: true, overRide:true);
                x+=speed.X;
                y+=speed.Y;
                if(twist!=0) {
                    if(randomtwist) {
                        twist+=WorldGen.genRand.NextFloat(-0.2f,0.2f);
                    }
                    speed = speed.RotatedBy(twist);
                }
                size--;
                if(speed.Length()>size*0.75f) {
                    speed.Normalize();
                    speed*=size*0.75f;
                }
            }
            return new Point((int)x,(int)y);
        }
        internal static int duskLeft;
        internal static int duskRight;
        internal static int duskTop;
        internal static int duskBottom;
        public static void HellRunner(int i, int j, double strength, int steps, int type, bool addTile = false, float speedX = 0f, float speedY = 0f, bool noYChange = false, bool overRide = true) {
	        double num = strength;
	        float num2 = steps;
	        Vector2 vector = default(Vector2);
	        vector.X = i;
	        vector.Y = j;
	        Vector2 vector2 = default(Vector2);
	        vector2.X = (float)WorldGen.genRand.Next(-10, 11) * 0.1f;
	        vector2.Y = (float)WorldGen.genRand.Next(-10, 11) * 0.1f;
	        if (speedX != 0f || speedY != 0f) {
		        vector2.X = speedX;
		        vector2.Y = speedY;
	        }
	        bool flag = type == 368;
	        bool flag2 = type == 367;
	        while (num > 0.0 && num2 > 0f) {
		        if (vector.Y < 0f && num2 > 0f && type == 59) {
			        num2 = 0f;
		        }
		        num = strength * (double)(num2 / (float)steps);
		        num2 -= 1f;
		        duskLeft = (int)((double)vector.X - num * 0.5);
		        duskRight = (int)((double)vector.X + num * 0.5);
		        duskTop = (int)((double)vector.Y - num * 0.5);
		        duskBottom = (int)((double)vector.Y + num * 0.5);
		        if (duskLeft < 1) {
			        duskLeft = 1;
		        }
		        if (duskRight > Main.maxTilesX - 1) {
			        duskRight = Main.maxTilesX - 1;
		        }
		        if (duskTop < Main.maxTilesY-200) {
			        duskTop = Main.maxTilesY-200;
		        }
		        if (duskBottom > Main.maxTilesY - 1) {
			        duskBottom = Main.maxTilesY - 1;
		        }
                int tilesSinceSpike = 0;
                Point? spike = null;
		        for (int k = duskLeft; k < duskRight; k++) {
                    spike = null;
			        for (int l = duskBottom; l > duskTop; l--) {
				        if (!((Math.Abs(k - vector.X) + Math.Abs(l - vector.Y)) < strength * 0.5)){
					        continue;
				        }
                        if(Main.tile[k, l].type == TileID.Pots) {
                            Main.tile[k, l].type = 0;
                            Main.tile[k, l].active(false);
                            continue;
                        }
                        if(Main.tile[k, l].type == type) {
                            continue;
                        }
                        if(Main.tile[k, l].liquid != 0) {
					        Tile tile = Main.tile[k, l];
						    tile.type = (ushort)type;
					        tile.active(active: true);
					        tile.liquid = 0;
					        tile.lava(lava: false);
                            //WorldGen.paintTile(k, l, 29);
                            Main.tile[k+1, l].slope(0);
                            Main.tile[k-1, l].slope(0);
                            Main.tile[k, l+1].slope(0);
                            Main.tile[k, l-1].slope(0);
                            if(Main.tile[k, l-1].liquid != 0)continue;
                            spike = new Point(k,l);
                            continue;
                        }
				        if ((Main.tile[k, l].active()) || Main.tile[k, l].wall == WallID.ObsidianBrickUnsafe || Main.tile[k, l].wall == WallID.HellstoneBrickUnsafe) {
					        Tile tile = Main.tile[k, l];
					        if (TileID.Sets.CanBeClearedDuringGeneration[tile.type]) {
                                if(tile.type == TileID.ObsidianBrick || tile.type == TileID.HellstoneBrick || tile.wall != 0 || (!Main.tileSolid[tile.type] && tile.type != TileID.Containers && tile.type != TileID.Containers2)) {
                                    tile.wall = 0;
                                    bool tileleft = Main.tile[k-1, l].type==type&&Main.tile[k-1, l].active();
                                    if(!tileleft && Main.tile[k, l-1].type != TileID.Containers && Main.tile[k, l-1].type != TileID.Containers2) {
                                        tile.type = 0;
                                        tile.active(false);
                                        continue;
                                    }
                                }
						        tile.type = (ushort)type;
					            tile.active(active: true);
					            //tile.liquid = 0;
					            //tile.lava(lava: false);
                                //WorldGen.paintTile(k, l, 29);
					        }
				        }
			        }
                    if(spike.HasValue) {
                        int l = spike.Value.Y;
                        if(WorldGen.genRand.Next(0, 10+OriginWorld.HellSpikes.Count)<=tilesSinceSpike/5) {
                            Origins.instance.Logger.Info("Adding spike @ "+k+", "+l);
                            OriginWorld.HellSpikes.Add((new Point(k, l), WorldGen.genRand.Next(5,10)+tilesSinceSpike/5));
                            //WorldGen.paintTile(k, l, 11);
                            tilesSinceSpike = -7;
                        } else {
                            tilesSinceSpike++;
                        }
                    }
		        }
		        vector += vector2;
		        if (num > 50.0) {
			        vector += vector2;
			        num2 -= 1f;
			        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
			        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
			        if (num > 100.0) {
				        vector += vector2;
				        num2 -= 1f;
				        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
				        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
				        if (num > 150.0) {
					        vector += vector2;
					        num2 -= 1f;
					        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
					        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
					        if (num > 200.0) {
						        vector += vector2;
						        num2 -= 1f;
						        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
						        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
						        if (num > 250.0) {
							        vector += vector2;
							        num2 -= 1f;
							        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
							        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
							        if (num > 300.0) {
								        vector += vector2;
								        num2 -= 1f;
								        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
								        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
								        if (num > 400.0) {
									        vector += vector2;
									        num2 -= 1f;
									        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
									        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
									        if (num > 500.0) {
										        vector += vector2;
										        num2 -= 1f;
										        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
										        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
										        if (num > 600.0) {
											        vector += vector2;
											        num2 -= 1f;
											        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
											        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
											        if (num > 700.0) {
												        vector += vector2;
												        num2 -= 1f;
												        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
												        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
												        if (num > 800.0) {
													        vector += vector2;
													        num2 -= 1f;
													        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
													        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
													        if (num > 900.0) {
														        vector += vector2;
														        num2 -= 1f;
														        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
														        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
													        }
												        }
											        }
										        }
									        }
								        }
							        }
						        }
					        }
				        }
			        }
		        }
		        vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
		        if (vector2.X > 1f) {
			        vector2.X = 1f;
		        }
		        if (vector2.X < -1f) {
			        vector2.X = -1f;
		        }
		        if (!noYChange) {
			        vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
			        if (vector2.Y > 1f) {
				        vector2.Y = 1f;
			        }
			        if (vector2.Y < -1f) {
				        vector2.Y = -1f;
			        }
		        }
		        else if (type != 59 && num < 3.0) {
			        if (vector2.Y > 1f) {
				        vector2.Y = 1f;
			        }
			        if (vector2.Y < -1f) {
				        vector2.Y = -1f;
			        }
		        }
		        if (type == 59 && !noYChange) {
			        if ((double)vector2.Y > 0.5) {
				        vector2.Y = 0.5f;
			        }
			        if ((double)vector2.Y < -0.5) {
				        vector2.Y = -0.5f;
			        }
			        if ((double)vector.Y < Main.rockLayer + 100.0) {
				        vector2.Y = 1f;
			        }
			        if (vector.Y > (float)(Main.maxTilesY - 300)) {
				        vector2.Y = -1f;
			        }
		        }
	        }
        }

    }
}
