using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Tyfyter.Utils {
	public static class FramingUtils {
		public static (ushort upLeft, ushort up, ushort upRight, ushort left, ushort right, ushort downLeft, ushort down, ushort downRight) GetAdjacentTileTypes(int i, int j) {
			Tile tile = Main.tile[i, j];

			Tile tileUp = Main.tile[i, j - 1];
			Tile tileDown = Main.tile[i, j + 1];
			Tile tileLeft = Main.tile[i - 1, j];
			Tile tileRight = Main.tile[i + 1, j];
			Tile tileDownLeft = Main.tile[i - 1, j + 1];
			Tile tileDownRight = Main.tile[i + 1, j + 1];
			Tile tileUpLeft = Main.tile[i - 1, j - 1];
			Tile tileUpRight = Main.tile[i + 1, j - 1];
			int upLeft = -1;
			int up = -1;
			int upRight = -1;
			int left = -1;
			int right = -1;
			int downLeft = -1;
			int down = -1;
			int downRight = -1;
			if (tileLeft != null && tileLeft.HasTile) {
				left = (Main.tileStone[tileLeft.TileType] ? 1 : tileLeft.TileType);
				if (tileLeft.Slope == SlopeType.SlopeDownLeft || tileLeft.Slope == SlopeType.SlopeUpLeft) {
					left = -1;
				}
			}
			if (tileRight != null && tileRight.HasTile) {
				right = (Main.tileStone[tileRight.TileType] ? 1 : tileRight.TileType);
				if (tileRight.Slope == SlopeType.SlopeDownRight || tileRight.Slope == SlopeType.SlopeUpRight) {
					right = -1;
				}
			}
			if (tileUp != null && tileUp.HasTile) {
				up = (Main.tileStone[tileUp.TileType] ? 1 : tileUp.TileType);
				if (tileUp.Slope == SlopeType.SlopeUpLeft || tileUp.Slope == SlopeType.SlopeUpRight) {
					up = -1;
				}
			}
			if (tileDown != null && tileDown.HasTile) {
				down = (Main.tileStone[tileDown.TileType] ? 1 : tileDown.TileType);
				if (tileDown.Slope == SlopeType.SlopeDownLeft || tileDown.Slope == SlopeType.SlopeDownRight) {
					down = -1;
				}
			}
			if (tileUpLeft != null && tileUpLeft.HasTile) {
				upLeft = (Main.tileStone[tileUpLeft.TileType] ? 1 : tileUpLeft.TileType);
			}
			if (tileUpRight != null && tileUpRight.HasTile) {
				upRight = (Main.tileStone[tileUpRight.TileType] ? 1 : tileUpRight.TileType);
			}
			if (tileDownLeft != null && tileDownLeft.HasTile) {
				downLeft = (Main.tileStone[tileDownLeft.TileType] ? 1 : tileDownLeft.TileType);
			}
			if (tileDownRight != null && tileDownRight.HasTile) {
				downRight = (Main.tileStone[tileDownRight.TileType] ? 1 : tileDownRight.TileType);
			}
			if (tile.Slope == SlopeType.SlopeDownRight) {
				up = -1;
				left = -1;
			}
			if (tile.Slope == SlopeType.SlopeDownLeft) {
				up = -1;
				right = -1;
			}
			if (tile.Slope == SlopeType.SlopeUpRight) {
				down = -1;
				left = -1;
			}
			if (tile.Slope == SlopeType.SlopeUpLeft) {
				down = -1;
				right = -1;
			}
			return ((ushort)upLeft, (ushort)up, (ushort)upRight, (ushort)left, (ushort)right, (ushort)downLeft, (ushort)down, (ushort)downRight);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="frameOption"></param>
		/// <param name="mergeDirections">upLeft, up, upRight, left, right, downLeft, down, downRight</param>
		public static void AutoFrame(int i, int j, int frameOption = -1, byte mergeDirections = 0) {
			Tile tile = Main.tile[i, j];
			int type = tile.TileType;
			int frameX = tile.TileFrameX;
			int frameY = tile.TileFrameY;
			if (frameOption == -1) {
				frameOption = WorldGen.genRand.Next(0, 3);
				tile.TileFrameNumber = (byte)frameOption;
			} else {
				frameOption = tile.TileFrameNumber;
			}
			if (Main.tileLargeFrames[type] == 1) {
				int num58 = j % 4;
				int num59 = i % 3;
				frameOption = (new int[4, 3]
				{
				{ 2, 4, 2 },
				{ 1, 3, 1 },
				{ 2, 2, 4 },
				{ 1, 1, 3 }
				})[num58, num59] - 1;
			}
			bool upLeft = (mergeDirections & 128) != 0;
			const byte up = 64;
			bool upRight = (mergeDirections & 32) != 0;
			const byte left = 16;
			const byte right = 8;
			bool downLeft = (mergeDirections & 4) != 0;
			const byte down = 2;
			bool downRight = (mergeDirections & 1) != 0;
			switch (mergeDirections) {
				case up | down | left | right: {
					if (!upLeft && !upRight) {
						switch (frameOption) {
							case 0:
							tile.TileFrameX = 108;
							tile.TileFrameY = 18;
							break;
							case 1:
							tile.TileFrameX = 126;
							tile.TileFrameY = 18;
							break;
							default:
							tile.TileFrameX = 144;
							tile.TileFrameY = 18;
							break;
						}
					} else if (!downLeft && !downRight) {
						switch (frameOption) {
							case 0:
							tile.TileFrameX = 108;
							tile.TileFrameY = 36;
							break;
							case 1:
							tile.TileFrameX = 126;
							tile.TileFrameY = 36;
							break;
							default:
							tile.TileFrameX = 144;
							tile.TileFrameY = 36;
							break;
						}
					} else if (!upLeft && !downLeft) {
						switch (frameOption) {
							case 0:
							tile.TileFrameX = 180;
							tile.TileFrameY = 0;
							break;
							case 1:
							tile.TileFrameX = 180;
							tile.TileFrameY = 18;
							break;
							default:
							tile.TileFrameX = 180;
							tile.TileFrameY = 36;
							break;
						}
					} else if (!upRight && !downRight) {
						switch (frameOption) {
							case 0:
							tile.TileFrameX = 198;
							tile.TileFrameY = 0;
							break;
							case 1:
							tile.TileFrameX = 198;
							tile.TileFrameY = 18;
							break;
							default:
							tile.TileFrameX = 198;
							tile.TileFrameY = 36;
							break;
						}
					} else {
						switch (frameOption) {
							case 0:
							tile.TileFrameX = 18;
							tile.TileFrameY = 18;
							break;
							case 1:
							tile.TileFrameX = 36;
							tile.TileFrameY = 18;
							break;
							default:
							tile.TileFrameX = 54;
							tile.TileFrameY = 18;
							break;
						}
					}
				}
				break;
				case down | left | right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 18;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 36;
						tile.TileFrameY = 0;
						break;
						default:
						tile.TileFrameX = 54;
						tile.TileFrameY = 0;
						break;
					}
				}
				break;
				case up | left | right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 18;
						tile.TileFrameY = 36;
						break;
						case 1:
						tile.TileFrameX = 36;
						tile.TileFrameY = 36;
						break;
						default:
						tile.TileFrameX = 54;
						tile.TileFrameY = 36;
						break;
					}
				}
				break;
				case up | down | right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 0;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 0;
						tile.TileFrameY = 18;
						break;
						default:
						tile.TileFrameX = 0;
						tile.TileFrameY = 36;
						break;
					}
				}
				break;
				case up | down | left: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 72;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 72;
						tile.TileFrameY = 18;
						break;
						default:
						tile.TileFrameX = 72;
						tile.TileFrameY = 36;
						break;
					}
				}
				break;
				case down | right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 0;
						tile.TileFrameY = 54;
						break;
						case 1:
						tile.TileFrameX = 36;
						tile.TileFrameY = 54;
						break;
						default:
						tile.TileFrameX = 72;
						tile.TileFrameY = 54;
						break;
					}
				}
				break;
				case down | left: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 18;
						tile.TileFrameY = 54;
						break;
						case 1:
						tile.TileFrameX = 54;
						tile.TileFrameY = 54;
						break;
						default:
						tile.TileFrameX = 90;
						tile.TileFrameY = 54;
						break;
					}
				}
				break;
				case up | right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 0;
						tile.TileFrameY = 72;
						break;
						case 1:
						tile.TileFrameX = 36;
						tile.TileFrameY = 72;
						break;
						default:
						tile.TileFrameX = 72;
						tile.TileFrameY = 72;
						break;
					}
				}
				break;
				case up | left: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 18;
						tile.TileFrameY = 72;
						break;
						case 1:
						tile.TileFrameX = 54;
						tile.TileFrameY = 72;
						break;
						default:
						tile.TileFrameX = 90;
						tile.TileFrameY = 72;
						break;
					}
				}
				break;
				case up | down: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 90;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 90;
						tile.TileFrameY = 18;
						break;
						default:
						tile.TileFrameX = 90;
						tile.TileFrameY = 36;
						break;
					}
				}
				break;
				case left | right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 108;
						tile.TileFrameY = 72;
						break;
						case 1:
						tile.TileFrameX = 126;
						tile.TileFrameY = 72;
						break;
						default:
						tile.TileFrameX = 144;
						tile.TileFrameY = 72;
						break;
					}
				}
				break;
				case down: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 108;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 126;
						tile.TileFrameY = 0;
						break;
						default:
						tile.TileFrameX = 144;
						tile.TileFrameY = 0;
						break;
					}
				}
				break;
				case up: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 108;
						tile.TileFrameY = 54;
						break;
						case 1:
						tile.TileFrameX = 126;
						tile.TileFrameY = 54;
						break;
						default:
						tile.TileFrameX = 144;
						tile.TileFrameY = 54;
						break;
					}
				}
				break;
				case right: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 162;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 162;
						tile.TileFrameY = 18;
						break;
						default:
						tile.TileFrameX = 162;
						tile.TileFrameY = 36;
						break;
					}
				}
				break;
				case left: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 216;
						tile.TileFrameY = 0;
						break;
						case 1:
						tile.TileFrameX = 216;
						tile.TileFrameY = 18;
						break;
						default:
						tile.TileFrameX = 216;
						tile.TileFrameY = 36;
						break;
					}
				}
				break;
				case 0: {
					switch (frameOption) {
						case 0:
						tile.TileFrameX = 162;
						tile.TileFrameY = 54;
						break;
						case 1:
						tile.TileFrameX = 180;
						tile.TileFrameY = 54;
						break;
						default:
						tile.TileFrameX = 198;
						tile.TileFrameY = 54;
						break;
					}
				}
				break;
			}
			if (tile.TileFrameX != frameX && tile.TileFrameY != frameY) {
				WorldGen.tileReframeCount++;
				if (WorldGen.tileReframeCount < 55) {
					WorldGen.TileFrame(i - 1, j);
					WorldGen.TileFrame(i + 1, j);
					WorldGen.TileFrame(i, j - 1);
					WorldGen.TileFrame(i, j + 1);
				}
				WorldGen.tileReframeCount--;
			}
		}
	}
}