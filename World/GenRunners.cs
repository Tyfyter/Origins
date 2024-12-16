using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using static Origins.World.AdjID;
using static Terraria.WorldGen;
using System.Collections.Generic;
using Origins.Tiles.Brine;
using Origins.Walls;
using Terraria.ModLoader;
using Microsoft.VisualBasic;

namespace Origins.World {
	public class GenRunners {
		public static Point SpikeRunner(int i, int j, int type, Vector2 speed, int size, float twist = 0, bool randomtwist = false) {
			float x = i;
			float y = j;
			while (size > 0) {
				WorldGen.TileRunner((int)x, (int)y, size, 2, type, speedX: speed.X, speedY: speed.Y, addTile: true, overRide: true);
				x += speed.X;
				y += speed.Y;
				if (twist != 0) {
					if (randomtwist) {
						twist += WorldGen.genRand.NextFloat(-0.2f, 0.2f);
					}
					speed = speed.RotatedBy(twist);
				}
				size--;
				if (speed.Length() > size * 0.75f) {
					speed.Normalize();
					speed *= size * 0.75f;
				}
			}
			return new Point((int)x, (int)y);
		}
		public static void DefiledSpikeRunner(int i, int j, double strength, int type, int ignoreWallType, Vector2 speed, double decay = 0, float twist = 0, bool randomtwist = false, bool forcesmooth = true, double cutoffStrength = 0.0) {
			double strengthLeft = strength;
			Vector2 pos = new Vector2(i, j);
			Tile tile;
			if (randomtwist) twist = Math.Abs(twist);
			int X0 = int.MaxValue;
			int X1 = 0;
			int Y0 = int.MaxValue;
			int Y1 = 0;
			while (strengthLeft > cutoffStrength) {
				strengthLeft -= decay;
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
						if (tile.HasTile ? TileID.Sets.CanBeClearedDuringGeneration[tile.TileType] : tile.WallType != ignoreWallType) {
							tile.TileType = (ushort)type;
							Main.tile[l, k].SetActive(true);
							Main.tile[l, k].LiquidAmount = 0;
							Main.tile[l, k].SetLiquidType(1);
							WorldGen.SquareTileFrame(l, k);
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
				if (forcesmooth && speed.Length() > strengthLeft * 0.75) {
					speed.Normalize();
					speed *= (float)strengthLeft;
				}
				pos += speed;
				if (randomtwist || twist != 0.0) {
					speed = randomtwist ? speed.RotatedBy(WorldGen.genRand.NextFloat(-twist, twist)) : speed.RotatedBy(twist);
				}
			}
			for (int l = X0; l < X1; l++) {
				for (int k = Y0; k < Y1; k++) {
					AutoSlopeForSpike(l, k);
				}
			}
			NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
		}
		public static void SmoothSpikeRunner(int i, int j, double strength, int type, Vector2 speed, double decay = 0, float twist = 0, bool randomtwist = false, bool forcesmooth = true, double cutoffStrength = 0.0) {
			double strengthLeft = strength;
			Vector2 pos = new Vector2(i, j);
			Tile tile;
			if (randomtwist) twist = Math.Abs(twist);
			int X0 = int.MaxValue;
			int X1 = 0;
			int Y0 = int.MaxValue;
			int Y1 = 0;
			while (strengthLeft > cutoffStrength) {
				strengthLeft -= decay;
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
						if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
							tile.TileType = (ushort)type;
							Main.tile[l, k].SetActive(true);
							Main.tile[l, k].LiquidAmount = 0;
							Main.tile[l, k].SetLiquidType(1);
							WorldGen.SquareTileFrame(l, k);
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
				if (forcesmooth && speed.Length() > strengthLeft * 0.75) {
					speed.Normalize();
					speed *= (float)strengthLeft;
				}
				pos += speed;
				if (randomtwist || twist != 0.0) {
					speed = randomtwist ? speed.RotatedBy(WorldGen.genRand.NextFloat(-twist, twist)) : speed.RotatedBy(twist);
				}
			}
			for (int l = X0; l < X1; l++) {
				for (int k = Y0; k < Y1; k++) {
					AutoSlopeForSpike(l, k);
				}
			}
			NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
		}
		public static int SpikeVeinRunner(int i, int j, double strength, HashSet<ushort> replacables, ushort type, Vector2 speed, double decay = 0, float twist = 0, bool randomTwist = false, bool forceSmooth = true, double cutoffStrength = 0.0) {
			int count = 0;
			double strengthLeft = strength;
			Vector2 pos = new(i, j);
			Tile tile;
			if (randomTwist) twist = Math.Abs(twist);
			int X0 = int.MaxValue;
			int X1 = 0;
			int Y0 = int.MaxValue;
			int Y1 = 0;
			if (decay <= 0) decay = 999;
			while (strengthLeft > cutoffStrength) {
				strengthLeft -= decay;
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
						if (tile.HasTile && replacables.Contains(tile.TileType)) {
							tile.TileType = type;
							tile.ClearBlockPaintAndCoating();
							count++;
							if (!generatingWorld) SquareTileFrame(l, k);
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
				if (forceSmooth && speed.Length() > strengthLeft * 0.75) {
					speed.Normalize();
					speed *= (float)strengthLeft;
				}
				pos += speed;
				if (randomTwist || twist != 0.0) {
					speed = randomTwist ? speed.RotatedBy(genRand.NextFloat(-twist, twist)) : speed.RotatedBy(twist);
				}
			}
			for (int l = X0; l < X1; l++) {
				for (int k = Y0; k < Y1; k++) {
					AutoSlopeForSpike(l, k);
				}
			}
			NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
			return count;
		}
		//take that, Heisenberg
		public static (Vector2 position, Vector2 velocity) VeinRunner(int i, int j, double strength, Vector2 speed, double length, float twist = 0, bool randomtwist = false) {
			Vector2 pos = new Vector2(i, j);
			Tile tile;
			if (randomtwist) twist = Math.Abs(twist);
			int X0 = int.MaxValue;
			int X1 = 0;
			int Y0 = int.MaxValue;
			int Y1 = 0;
			strength = Math.Pow(strength, 2);
			double decay = speed.Length();

			while (length > 0) {
				length -= decay;
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
						if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
							Main.tile[l, k].SetActive(false);
							//WorldGen.SquareTileFrame(l,k);
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
				if (twist != 0.0) {
					speed = speed.RotatedBy(twist);
				} else if (randomtwist) {
					speed = speed.RotatedBy(WorldGen.genRand.NextFloat(-twist, twist));
				}
			}
#if DEBUG
			//Main.tile[(int)pos.X, (int)pos.Y].wall = WallID.EmeraldGemspark;
#endif
			WorldGen.RangeFrame(X0, Y0, X1, Y1);
			NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
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
			bool hasWall = wallType != -1;
			ushort _wallType = hasWall ? (ushort)wallType : (ushort)0;
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
							if (d < baseStrength + wallThickness && TileID.Sets.CanBeClearedDuringGeneration[tile.TileType] && tile.WallType != _wallType) {
								tile.HasTile = true;
								tile.ResetToType(wallBlockType);
								//WorldGen.SquareTileFrame(l, k);
								if (hasWall) {
									tile.WallType = _wallType;
								}
							}
							continue;
						}
						if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
							Main.tile[l, k].SetActive(false);
							//WorldGen.SquareTileFrame(l, k);
							if (hasWall) {
								tile.WallType = _wallType;
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
			NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
			return (pos, speed);
		}
		public static (Vector2 position, Vector2 velocity) OpeningRunner(int i, int j, double strength, double strengthGrowth, Vector2 speed, double length, bool[] validTiles = null) {
			if (validTiles is null) validTiles = TileID.Sets.CanBeClearedDuringGeneration.ToArray();
			Vector2 pos = new Vector2(i, j);
			Tile tile;
			int X0 = int.MaxValue;
			int X1 = 0;
			int Y0 = int.MaxValue;
			int Y1 = 0;
			strength = Math.Pow(strength, 2);
			double edgeStrength = Math.Pow(strength + 4, 2);
			double decay = speed.Length();
			int clearedCount = 1;
			ushort stoneID = (ushort)ModContent.TileType<Sulphur_Stone>();
			ushort stoneWallID = (ushort)ModContent.WallType<Sulphur_Stone_Wall>();
			while (clearedCount > 0 && length > 0) {
				length -= decay;
				int minX = (int)(pos.X - edgeStrength * 0.5);
				int maxX = (int)(pos.X + edgeStrength * 0.5);
				int minY = (int)(pos.Y - edgeStrength * 0.5);
				int maxY = (int)(pos.Y + edgeStrength * 0.5);
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
				clearedCount = 0;
				for (int l = minX; l < maxX; l++) {
					for (int k = minY; k < maxY; k++) {
						double dist = (Math.Pow(Math.Abs(l - pos.X), 2) + Math.Pow(Math.Abs(k - pos.Y), 2));
						tile = Main.tile[l, k];
						if (dist > strength) {//if (!((Math.Abs(l - pos.X) + Math.Abs(k - pos.Y)) < strength)) {
							if (dist < edgeStrength && tile.HasTile && validTiles[tile.TileType] && WorldGen.CanKillTile(l, k)) {
								tile.TileType = stoneID;
								tile.WallType = stoneWallID;
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
							continue;
						}
						if (tile.HasTile && validTiles[tile.TileType] && WorldGen.CanKillTile(l, k)) {
							tile.HasTile = false;
							tile.WallType = stoneWallID;
							clearedCount++;
							//WorldGen.SquareTileFrame(l,k);
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
				strength += decay * strengthGrowth;
				edgeStrength = Math.Sqrt(strength) + 1;
				edgeStrength *= edgeStrength;
			}
			WorldGen.RangeFrame(X0, Y0, X1, Y1);
			NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
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
						if (tile.TileType == TileID.Cloud || tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass || tile.TileType == TileID.Stone || tile.TileType == TileID.RainCloud || tile.TileType == TileID.Stone) {
							tile.TileType = (ushort)type;
							if (!tile.HasTile && OriginSystem.GetAdjTileCount(k, l) > 3) {
								tile.HasTile = true;
							}
							SquareTileFrame(k, l);
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
		public static int DictionaryRunner(int i, int j, double strength, int steps, Dictionary<ushort, ushort> types, float speedX = 0f, float speedY = 0f) {
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
			int totalChanged = 0;
			while (currStrength > 0.0 && step > 0f) {
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
				int changedCount = 0;
				for (int k = num3; k < num4; k++) {
					for (int l = num5; l < num6; l++) {
						if (!((Math.Abs(k - pos.X) + Math.Abs(l - pos.Y)) < strength * 0.5 * (1.0 + genRand.Next(-10, 11) * 0.015))) {
							continue;
						}
						Tile tile = Main.tile[k, l];
						if (types.TryGetValue(tile.TileType, out ushort type)) {
							tile.TileType = type;
							if (!tile.HasTile && OriginSystem.GetAdjTileCount(k, l) > 3) {
								tile.HasTile = true;
							}
							SquareTileFrame(k, l);
							if (Main.netMode == NetmodeID.Server) {
								NetMessage.SendTileSquare(-1, k, l, 1);
							}
						}
					}
				}
				if (changedCount != 0) {
					break;
				} else {
					totalChanged += changedCount;
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
			return totalChanged;
		}
		public static int DictionaryWallRunner(int i, int j, double strength, int steps, Dictionary<ushort, ushort> types, float speedX = 0f, float speedY = 0f) {
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
			int totalChanged = 0;
			while (currStrength > 0.0 && step > 0f) {
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
				int changedCount = 0;
				for (int k = num3; k < num4; k++) {
					for (int l = num5; l < num6; l++) {
						if (!((Math.Abs(k - pos.X) + Math.Abs(l - pos.Y)) < strength * 0.5 * (1.0 + genRand.Next(-10, 11) * 0.015))) {
							continue;
						}
						Tile tile = Main.tile[k, l];
						if (types.TryGetValue(tile.WallType, out ushort type)) {
							tile.WallType = type;
							changedCount++;
							SquareTileFrame(k, l);
							if (Main.netMode == NetmodeID.Server) {
								NetMessage.SendTileSquare(-1, k, l, 1);
							}
						}
					}
				}
				if (changedCount == 0) {
					break;
				} else {
					totalChanged += changedCount;
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
			return totalChanged;
		}
		public static void AutoSlope(int i, int j, bool resetSlope = false) {
			byte adj = 0;
			Tile tile = Main.tile[i, j];
			if (resetSlope) {
				tile.IsHalfBlock = false;
				tile.Slope = SlopeID.None;
			}
			if (Main.tile[i - 1, j - 1].HasTile) adj |= tl;
			if (Main.tile[i, j - 1].HasTile) adj |= t;
			if (Main.tile[i + 1, j - 1].HasTile) adj |= tr;
			if (Main.tile[i - 1, j].HasTile) adj |= l;
			if (Main.tile[i + 1, j].HasTile) adj |= r;
			if (Main.tile[i - 1, j + 1].HasTile) adj |= bl;
			if (Main.tile[i, j + 1].HasTile) adj |= b;
			if (Main.tile[i + 1, j + 1].HasTile) adj |= br;
			bool sloped = true;
			retry:
			switch (adj) {
				case bl | b | br:
				tile.IsHalfBlock = true;
				break;
				case t | l:
				tile.Slope = SlopeType.SlopeUpLeft;
				break;
				case t | r:
				tile.Slope = SlopeType.SlopeUpRight;
				break;
				case b | l:
				tile.Slope = SlopeType.SlopeDownLeft;
				break;
				case b | r:
				tile.Slope = SlopeType.SlopeDownRight;
				break;
				default:
				if (sloped) {
					sloped = false;
					adj &= t | l | r | b;
					goto retry;
				}
				break;
			}
		}
		public static void AutoSlopeForSpike(int i, int j) {
			byte adj = 0;
			Tile tile = Main.tile[i, j];
			tile.IsHalfBlock = false;
			tile.Slope = SlopeID.None;
			if (Main.tile[i - 1, j - 1].HasTile) adj |= tl;
			if (Main.tile[i, j - 1].HasTile) adj |= t;
			if (Main.tile[i + 1, j - 1].HasTile) adj |= tr;
			if (Main.tile[i - 1, j].HasTile) adj |= l;
			if (Main.tile[i + 1, j].HasTile) adj |= r;
			if (Main.tile[i - 1, j + 1].HasTile) adj |= bl;
			if (Main.tile[i, j + 1].HasTile) adj |= b;
			if (Main.tile[i + 1, j + 1].HasTile) adj |= br;
			bool sloped = true;
			retry:
			switch (adj) {
				case bl | b | br:
				tile.IsHalfBlock = true;
				break;
				case t | l:
				tile.Slope = SlopeType.SlopeUpLeft;
				break;
				case t | r:
				tile.Slope = SlopeType.SlopeUpRight;
				break;
				case b | l:
				tile.Slope = SlopeType.SlopeDownLeft;
				break;
				case b | r:
				tile.Slope = SlopeType.SlopeDownRight;
				break;
				case b | bl:
				tile.Slope = SlopeType.SlopeDownRight;
				break;
				case b | br:
				tile.Slope = SlopeType.SlopeDownLeft;
				break;
				case t | tl:
				tile.Slope = SlopeType.SlopeUpRight;
				break;
				case t | tr:
				tile.Slope = SlopeType.SlopeUpLeft;
				break;
				case l | tl:
				tile.Slope = SlopeType.SlopeDownLeft;
				break;
				case l | bl:
				tile.Slope = SlopeType.SlopeUpLeft;
				break;
				case r | tr:
				tile.Slope = SlopeType.SlopeDownLeft;
				break;
				case r | br:
				tile.Slope = SlopeType.SlopeDownRight;
				break;
				default:
				if (sloped) {
					sloped = false;
					adj &= t | l | r | b;
					goto retry;
				}
				break;
			}
		}
		/// <summary>
		/// When I wrote this code, only God knew how it worked, that fact has not changed
		/// </summary>
		/// <param name="value">the x value along the mostly continuous function</param>
		/// <returns>mostly continuous noise based on the value of x, may have some near-looping period</returns>
		public static float GetWallDistOffset(float value) {//TODO: replace with more performant pseudorandom algorithm, this is responsible for most of the time it takes to generate the riven
			float x = value * 0.4f;
			float halfx = x * 0.5f;
			float quarx = x * 0.5f;
			if (value < 0) {
				float nx0 = -MathF.Min(MathF.Pow(-halfx % 3, halfx % 5), 2);
				halfx += 0.5f;
				float nx1;
				if (halfx < 0) {
					nx1 = nx0;
				} else {
					nx1 = MathF.Min(MathF.Pow(halfx % 3, halfx % 5), 2);
				}
				float nx2 = nx0 * (-MathF.Min(MathF.Pow(-quarx % 3, quarx % 5), 2) + 0.5f);
				return nx0 - nx2 + nx1;
			}
			float fx0 = MathF.Min(MathF.Pow(halfx % 3, halfx % 5), 2);
			halfx += 0.5f;
			float fx1 = MathF.Min(MathF.Pow(halfx % 3, halfx % 5), 2);
			float fx2 = fx0 * (MathF.Min(MathF.Pow(quarx % 3, quarx % 5), 2) + 0.5f);
			return fx0 - fx2 + fx1;
		}
	}
	public static class AdjID {
		public const byte tl = 1;
		public const byte t = 2;
		public const byte tr = 4;
		public const byte l = 8;
		public const byte r = 16;
		public const byte bl = 32;
		public const byte b = 64;
		public const byte br = 128;
	}
}
