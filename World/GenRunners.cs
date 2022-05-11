using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using static Origins.World.AdjID;
using static Terraria.WorldGen;

namespace Origins.World {
    public class GenRunners{
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
        if(l<Main.maxTilesY)Main.tile[k, l+1].slope(0);
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
        public static void SmoothSpikeRunner(int i, int j, double strength, int type, Vector2 speed, double decay = 0, float twist = 0, bool randomtwist = false, bool forcesmooth = true, double cutoffStrength = 0.0){
	        double strengthLeft = strength;
	        Vector2 pos = new Vector2(i,j);
            Tile tile;
            if(randomtwist)twist = Math.Abs(twist);
            int X0 = int.MaxValue;
            int X1 = 0;
            int Y0 = int.MaxValue;
            int Y1 = 0;
            while (strengthLeft > cutoffStrength) {
		        strengthLeft-=decay;
		        int minX = (int)(pos.X - strengthLeft * 0.5);
		        int maxX = (int)(pos.X + strengthLeft * 0.5);
		        int minY = (int)(pos.Y - strengthLeft * 0.5);
		        int maxY = (int)(pos.Y + strengthLeft * 0.5);
		        if (minX < 1) {
			        minX = 1;
		        }
		        if (maxX > Main.maxTilesX - 1) {
			        maxX = Main.maxTilesX - 1;
		        }
		        if (minY < 1) {
			        minY = 1;
		        }
		        if (maxY > Main.maxTilesY - 1) {
			        maxY = Main.maxTilesY - 1;
		        }
		        for (int l = minX; l < maxX; l++) {
			        for (int k = minY; k < maxY; k++) {
				        if ((Math.Pow(Math.Abs(l - pos.X), 2) + Math.Pow(Math.Abs(k - pos.Y), 2)) > Math.Pow(strength, 2)) {//if (!((Math.Abs(l - pos.X) + Math.Abs(k - pos.Y)) < strength)) {
					        continue;
				        }
					    tile = Main.tile[l, k];
					    if (TileID.Sets.CanBeClearedDuringGeneration[tile.type]) {
							tile.type = (ushort)type;
					        Main.tile[l, k].active(active: true);
					        Main.tile[l, k].liquid = 0;
					        Main.tile[l, k].lava(lava: false);
                            WorldGen.SquareTileFrame(l,k);
                            if(l>X1) {
                                X1 = l;
                            }else if(l<X0) {
                                X0 = l;
                            }
                            if(k>Y1) {
                                Y1 = k;
                            }else if(k<Y0) {
                                Y0 = k;
                            }
					    }
			        }
		        }
                if(forcesmooth&&speed.Length()>strengthLeft*0.75) {
                    speed.Normalize();
                    speed*=(float)strengthLeft;
                }
		        pos += speed;
                if(randomtwist||twist!=0.0) {
                    speed = randomtwist?speed.RotatedBy(WorldGen.genRand.NextFloat(-twist,twist)):speed.RotatedBy(twist);
                }
	        }
            float r = speed.ToRotation();
            for(int l = X0; l < X1; l++) {
                for(int k = Y0; k < Y1; k++) {
                    AutoSlopeForSpike(l, k);
                }
            }
            NetMessage.SendTileRange(Main.myPlayer, X0, Y0, X1-X0, Y1-Y1);
        }
        //take that, Heisenberg
        public static (Vector2 position, Vector2 velocity) VeinRunner(int i, int j, double strength, Vector2 speed, double length, float twist = 0, bool randomtwist = false){
	        Vector2 pos = new Vector2(i,j);
            Tile tile;
            if(randomtwist)twist = Math.Abs(twist);
            int X0 = int.MaxValue;
            int X1 = 0;
            int Y0 = int.MaxValue;
            int Y1 = 0;
            strength = Math.Pow(strength, 2);
            double decay = speed.Length();
            while (length > 0) {
		        length-=decay;
		        int minX = (int)(pos.X - strength * 0.5);
		        int maxX = (int)(pos.X + strength * 0.5);
		        int minY = (int)(pos.Y - strength * 0.5);
		        int maxY = (int)(pos.Y + strength * 0.5);
		        if (minX < 1) {
			        minX = 1;
		        }
		        if (maxX > Main.maxTilesX - 1) {
			        maxX = Main.maxTilesX - 1;
		        }
		        if (minY < 1) {
			        minY = 1;
		        }
		        if (maxY > Main.maxTilesY - 1) {
			        maxY = Main.maxTilesY - 1;
		        }
#if DEBUG
				//Main.tile[(int)pos.X, (int)pos.Y].wall = WallID.AmberGemspark;
#endif
		        for (int l = minX; l < maxX; l++) {
			        for (int k = minY; k < maxY; k++) {
				        if ((Math.Pow(Math.Abs(l - pos.X), 2) + Math.Pow(Math.Abs(k - pos.Y), 2)) > strength) {//if (!((Math.Abs(l - pos.X) + Math.Abs(k - pos.Y)) < strength)) {
					        continue;
				        }
					    tile = Main.tile[l, k];
					    if (TileID.Sets.CanBeClearedDuringGeneration[tile.type]){
					        Main.tile[l, k].active(active: false);
                            //WorldGen.SquareTileFrame(l,k);
                            if(l>X1) {
                                X1 = l;
                            }else if(l<X0) {
                                X0 = l;
                            }
                            if(k>Y1) {
                                Y1 = k;
                            }else if(k<Y0) {
                                Y0 = k;
                            }
					    }
			        }
		        }
		        pos += speed;
                if(randomtwist||twist!=0.0) {
                    speed = randomtwist?speed.RotatedBy(WorldGen.genRand.NextFloat(-twist,twist)):speed.RotatedBy(twist);
                }
	        }
#if DEBUG
			//Main.tile[(int)pos.X, (int)pos.Y].wall = WallID.EmeraldGemspark;
#endif
			WorldGen.RangeFrame(X0, Y0, X1, Y1);
			NetMessage.SendTileRange(Main.myPlayer, X0, Y0, X1-X0, Y1-Y1);
            return (pos, speed);
        }
		public static (Vector2 position, Vector2 velocity) WalledVeinRunner(int i, int j, double strength, Vector2 speed, double length, ushort wallBlockType, float wallThickness, float twist = 0, bool randomtwist = false, int wallType = -1) {
			Vector2 pos = new Vector2(i, j);
			Tile tile;
			if (randomtwist) twist = Math.Abs(twist);
			int X0 = int.MaxValue;
			int X1 = 0;
			int Y0 = int.MaxValue;
			int Y1 = 0;
			double baseStrength = strength;
			strength = Math.Pow(strength, 2);
			wallThickness *= wallThickness;
			double decay = speed.Length();
			bool hasWall = wallType!=-1;
			ushort _wallType = hasWall?(ushort)wallType:(ushort)0;
			while (length > 0) {
				length -= decay;
				int minX = (int)(pos.X - (strength + wallThickness) * 0.5);
				int maxX = (int)(pos.X + (strength + wallThickness) * 0.5);
				int minY = (int)(pos.Y - (strength + wallThickness) * 0.5);
				int maxY = (int)(pos.Y + (strength + wallThickness) * 0.5);
				if (minX < 1) {
					minX = 1;
				}
				if (maxX > Main.maxTilesX - 1) {
					maxX = Main.maxTilesX - 1;
				}
				if (minY < 1) {
					minY = 1;
				}
				if (maxY > Main.maxTilesY - 1) {
					maxY = Main.maxTilesY - 1;
				}
#if DEBUG
				//Main.tile[(int)pos.X, (int)pos.Y].wall = WallID.AmberGemspark;
#endif
				for (int l = minX; l < maxX; l++) {
					for (int k = minY; k < maxY; k++) {
						double dist = (Math.Pow(Math.Abs(l - pos.X), 2) + Math.Pow(Math.Abs(k - pos.Y), 2));
						tile = Main.tile[l, k];
						if (dist > strength) {
							double d = Math.Sqrt(dist);
							if (d < baseStrength + wallThickness && TileID.Sets.CanBeClearedDuringGeneration[tile.type] && tile.wall != _wallType) {
								tile.active(active: true);
								tile.ResetToType(wallBlockType);
								//WorldGen.SquareTileFrame(l, k);
								if (hasWall) {
									tile.wall = _wallType;
								}
							}
							continue;
						}
						if (TileID.Sets.CanBeClearedDuringGeneration[tile.type]) {
							Main.tile[l, k].active(active: false);
							//WorldGen.SquareTileFrame(l, k);
							if (hasWall) {
								tile.wall = _wallType;
							}
							if (l > X1) {
								X1 = l;
							} else if (l < X0) {
								X0 = l;
							}
							if (k > Y1) {
								Y1 = k;
							} else if (k < Y0) {
								Y0 = k;
							}
						}
					}
				}
				pos += speed;
				if (randomtwist || twist != 0.0) {
					speed = randomtwist ? speed.RotatedBy(WorldGen.genRand.NextFloat(-twist, twist)) : speed.RotatedBy(twist);
				}
			}
#if DEBUG
			//Main.tile[(int)pos.X, (int)pos.Y].wall = WallID.EmeraldGemspark;
#endif
			if (X0 < 1) {
				X0 = 1;
			}
			if (Y0 > Main.maxTilesX - 1) {
				Y0 = Main.maxTilesX - 1;
			}
			if (X1 < 1) {
				X1 = 1;
			}
			if (Y1 > Main.maxTilesY - 1) {
				Y1 = Main.maxTilesY - 1;
			}
			WorldGen.RangeFrame(X0, Y0, X1, Y1);
			NetMessage.SendTileRange(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
			return (pos, speed);
		}
		public static void FelnumRunner(int i, int j, double strength, int steps, int type, float speedX = 0f, float speedY = 0f) {
			double currStrength = strength;
			float step = steps;
			Vector2 pos = default;
			pos.X = i;
			pos.Y = j;
			Vector2 speed = default;
			speed.X = genRand.Next(-10, 11) * 0.1f;
			speed.Y = genRand.Next(-10, 11) * 0.1f;
			if (speedX != 0f || speedY != 0f) {
				speed.X = speedX;
				speed.Y = speedY;
			}
			while (currStrength > 0.0 && step > 0f) {
				if (pos.Y < 0f && step > 0f && type == 59) {
					step = 0f;
				}
				currStrength = strength * (step / steps);
				step -= 1f;
				int num3 = (int)(pos.X - currStrength * 0.5);
				int num4 = (int)(pos.X + currStrength * 0.5);
				int num5 = (int)(pos.Y - currStrength * 0.5);
				int num6 = (int)(pos.Y + currStrength * 0.5);
				if (num3 < 1) {
					num3 = 1;
				}
				if (num4 > Main.maxTilesX - 1) {
					num4 = Main.maxTilesX - 1;
				}
				if (num5 < 1) {
					num5 = 1;
				}
				if (num6 > Main.maxTilesY - 1) {
					num6 = Main.maxTilesY - 1;
				}
				for (int k = num3; k < num4; k++) {
					for (int l = num5; l < num6; l++) {
						if (!((Math.Abs(k - pos.X) + Math.Abs(l - pos.Y)) < strength * 0.5 * (1.0 + genRand.Next(-10, 11) * 0.015))) {
							continue;
						}
						Tile tile = Main.tile[k, l];
						if (tile.type==TileID.Cloud||tile.type==TileID.Dirt||tile.type==TileID.Grass||tile.type==TileID.Stone||tile.type==TileID.RainCloud||tile.type==TileID.Stone) {
							tile.type = (ushort)type;
                            if(!tile.active()&&OriginWorld.GetAdjTileCount(k,l)>3) {
                                tile.active(true);
                            }
                            SquareTileFrame(k,l);
					        if (Main.netMode == NetmodeID.Server) {
						        NetMessage.SendTileSquare(-1, k, l, 1);
					        }
						}
					}
				}
				pos += speed;
				speed.X += genRand.Next(-10, 11) * 0.05f;
				if (speed.X > 1f) {
					speed.X = 1f;
				}
				if (speed.X < -1f) {
					speed.X = -1f;
				}
				speed.Y += genRand.Next(-10, 11) * 0.05f;
				if (speed.Y > 1f) {
					speed.Y = 1f;
				}
				if (speed.Y < -1f) {
					speed.Y = -1f;
				}
			}
		}
        public static void AutoSlope(int i, int j, bool resetSlope = false) {
            byte adj = 0;
			Tile tile = Main.tile[i, j];
            if(resetSlope) {
                tile.halfBrick(false);
                tile.slope(SlopeID.None);
            }
            if(Main.tile[i-1, j-1].active())adj|=tl;
            if(Main.tile[i, j-1].active())  adj|=t;
            if(Main.tile[i+1, j-1].active())adj|=tr;
            if(Main.tile[i-1, j].active())  adj|=l;
            if(Main.tile[i+1, j].active())  adj|=r;
            if(Main.tile[i-1, j+1].active())adj|=bl;
            if(Main.tile[i, j+1].active())  adj|=b;
            if(Main.tile[i+1, j+1].active())adj|=br;
            bool sloped = true;
            retry:
            switch(adj) {
                case bl|b|br:
                tile.halfBrick(true);
                break;
                case t|l:
                tile.slope(SlopeID.TopLeft);
                break;
                case t|r:
                tile.slope(SlopeID.TopRight);
                break;
                case b|l:
                tile.slope(SlopeID.BottomLeft);
                break;
                case b|r:
                tile.slope(SlopeID.BottomRight);
                break;
                default:
                if(sloped) {
                    sloped = false;
                    adj&=t|l|r|b;
                    goto retry;
                }
                break;
            }
        }
        public static void AutoSlopeForSpike(int i, int j) {
            byte adj = 0;
			Tile tile = Main.tile[i, j];
            tile.halfBrick(false);
            tile.slope(SlopeID.None);
            if(Main.tile[i-1, j-1].active())adj|=tl;
            if(Main.tile[i, j-1].active())  adj|=t;
            if(Main.tile[i+1, j-1].active())adj|=tr;
            if(Main.tile[i-1, j].active())  adj|=l;
            if(Main.tile[i+1, j].active())  adj|=r;
            if(Main.tile[i-1, j+1].active())adj|=bl;
            if(Main.tile[i, j+1].active())  adj|=b;
            if(Main.tile[i+1, j+1].active())adj|=br;
            bool sloped = true;
            retry:
            switch(adj) {
                case bl|b|br:
                tile.halfBrick(true);
                break;
                case t|l:
                tile.slope(SlopeID.TopLeft);
                break;
                case t|r:
                tile.slope(SlopeID.TopRight);
                break;
                case b|l:
                tile.slope(SlopeID.BottomLeft);
                break;
                case b|r:
                tile.slope(SlopeID.BottomRight);
                break;
                case b|bl:
                tile.slope(SlopeID.BottomRight);
                break;
                case b|br:
                tile.slope(SlopeID.BottomLeft);
                break;
                case t|tl:
                tile.slope(SlopeID.TopRight);
                break;
                case t|tr:
                tile.slope(SlopeID.TopLeft);
                break;
                case l|tl:
                tile.slope(SlopeID.BottomLeft);
                break;
                case l|bl:
                tile.slope(SlopeID.TopLeft);
                break;
                case r|tr:
                tile.slope(SlopeID.BottomRight);
                break;
                case r|br:
                tile.slope(SlopeID.BottomLeft);
                break;
                default:
                if(sloped) {
                    sloped = false;
                    adj&=t|l|r|b;
                    goto retry;
                }
                break;
            }
        }
		/// <summary>
		/// When I wrote this code, only God knew how it worked, that fact has not changed
		/// </summary>
		/// <param name="value">the x value along the continuous function</param>
		/// <returns>mostly continuous noise based on the value of x, may have some near-looping period</returns>
		public static float GetWallDistOffset(float value) {
			float x = value * 0.4f;
			float halfx = x * 0.5f;
			float quarx = x * 0.5f;
            if (value < 0) {
				float nx0 = (float)-Math.Min(Math.Pow(-halfx % 3, halfx % 5), 2);
				halfx += 0.5f;
				float nx1;
				if (halfx < 0) {
					nx1 = (float)-Math.Min(Math.Pow(-halfx % 3, halfx % 5), 2);
				}else{
					nx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				}
				float nx2 = nx0 * (float)(-Math.Min(Math.Pow(-quarx % 3, quarx % 5), 2) + 0.5f);
				return nx0 - nx2 + nx1;
            }
			float fx0 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
			halfx += 0.5f;
			float fx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
			float fx2 = fx0 * (float)(Math.Min(Math.Pow(quarx % 3, quarx % 5), 2) + 0.5f);
			return fx0 - fx2 + fx1;
		}
    }
    public static class AdjID {
        public const byte tl = 1;
        public const byte t =  2;
        public const byte tr = 4;
        public const byte l =  8;
        public const byte r =  16;
        public const byte bl = 32;
        public const byte b =  64;
        public const byte br = 128;
    }
}
